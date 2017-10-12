using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.Extensions.Options;
using System.Threading;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Services.Analytics.Models;
using System.Net.Http;

namespace Services.Analytics
{
    public class MediaContext
    {
        private readonly ZetronContext _dbContext;
        private readonly AppSettings _appSettings;

        private readonly CloudMediaContext _mediaContext;
        private readonly MediaServicesCredentials _cachedCredentials;
        private readonly IChannel _channel;


        public MediaContext(ZetronContext context, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _dbContext = context;

            try
            {
                _cachedCredentials = new MediaServicesCredentials(_appSettings.MediaServicesAccountName, _appSettings.MediaServicesAccountKey);
                // Used the cached credentials to create CloudMediaContext.
                _mediaContext = new CloudMediaContext(_cachedCredentials);
                _channel = _mediaContext.Channels.FirstOrDefault();
            }
            catch (Exception Ex)
            {
                Log("MediaContext - Unable to get Azure media context. Exception: " + Ex.Message);
                throw;
            }
        }

        public async void StartAzureProcess(ZetronMstIncidents zetronMstIncidents)
        {
            try
            {
                await Task.Run(() =>
                {
                    //Get first channel
                    StartChannel();

                    if (_channel != null)
                    {
                        ILocator originLocator;
                        //Create asset for storage
                        IAsset asset = CreateAndConfigureAsset(zetronMstIncidents, out originLocator);
                        if (asset != null)
                        {
                            //Create and start live program and configure storage for it
                            CreateAndStartProgram(asset, originLocator, zetronMstIncidents);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        public async void StopAzureProcess()
        {
            try
            {
                await Task.Yield();
                var livePrograms = _channel.Programs.ToList();
                List<Task> tasks = new List<Task>();                
                foreach (var program in livePrograms)
                {
                    if (program.State == ProgramState.Running)
                        tasks.Add(program.StopAsync());
                }                
                Task.WaitAll(tasks.ToArray());                                
                livePrograms.ForEach(p => p.DeleteAsync());
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        public async void StopChannel()
        {
            try
            {
                if (_channel.State == ChannelState.Running || _channel.State == ChannelState.Starting)
                    _channel.StopAsync();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        Task channelstart;
        private async void StartChannel()
        {
            try
            {
                //If the channel is already started do nothing
                if (_channel != null && !(_channel.State == ChannelState.Running || _channel.State == ChannelState.Starting))
                {
                    channelstart = _channel.StartAsync();
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        private IAsset CreateAndConfigureAsset(ZetronMstIncidents zetronMstIncidents, out ILocator originLocator)
        {
            //Create asset
            IAsset asset = null;
            try
            {
                asset = _mediaContext.Assets.Create("Asset_" + zetronMstIncidents.IncidentId.ToString(), AssetCreationOptions.None);

                // Create a 30 - day readonly access policy. 
                IAccessPolicy accessPolicy = _mediaContext.AccessPolicies.Create("Streaming policy",
                    TimeSpan.FromDays(30),
                    AccessPermissions.Read);

                // Create a locator to the streaming content on an origin. 
                originLocator = _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset,
                    accessPolicy,
                    DateTime.UtcNow.AddMinutes(-5));
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
            
            return asset;
        }

        private async void CreateAndStartProgram(IAsset asset, ILocator originLocator, ZetronMstIncidents zetronMstIncidents)
        {
            try
            {
                //Create a new program and allocate asset for it.
                //To Do: Check the amount of time the asset will be maintained
                IProgram program = await _channel.Programs.CreateAsync("Program-" + zetronMstIncidents.IncidentId.ToString(), TimeSpan.FromHours(8), asset.Id);

                if (channelstart != null)
                    Task.WaitAll(channelstart);

                if (_channel.State == ChannelState.Running)
                {
                    //Start the program
                    await program.StartAsync();
                    UpdateStreamingUrl(asset, originLocator, zetronMstIncidents);

                    if (zetronMstIncidents.Status == (int)IncidentStatus.Started)
                    {
                        TriggerJob(zetronMstIncidents.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        private async void UpdateStreamingUrl(IAsset asset, ILocator originLocator, ZetronMstIncidents zetronMstIncidents)
        {
            try
            {
                //Get manifest file to create streaming url
                IAssetFile manifestFile = asset.AssetFiles.Where(f => f.Name.ToLower().EndsWith(".ism")).FirstOrDefault();
                string streamUrl = originLocator.Path + manifestFile.Name + "/manifest(format=m3u8-aapl)";

                ZetronTrnMediaDetails mediaDetails = new ZetronTrnMediaDetails()
                {
                    IncidentId = zetronMstIncidents.IncidentId,
                    MediaUrl = streamUrl,
                    MediaType = 1,
                    PostedIon = DateTime.Now.ToUniversalTime(),
                    PostedBy = "Admin",
                    Status = true
                };

                _dbContext.ZetronTrnMediaDetails.Add(mediaDetails);

                ZetronMstIncidents recordtoUpdate = null;
                if (zetronMstIncidents.Status == (int)IncidentStatus.Started)
                {
                    recordtoUpdate = _dbContext.ZetronMstIncidents.Single(i => i.IncidentId == zetronMstIncidents.IncidentId);
                    recordtoUpdate.Status = (int)IncidentStatus.Processing;
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        private void Log(string action, string entityId = null, string operationId = null)
        {
            Console.WriteLine(
                "{0,-21}{1,-51}{2,-51}{3,-51}",
                DateTime.Now.ToString("yyyy'-'MM'-'dd HH':'mm':'ss"),
                action,
                entityId ?? string.Empty,
                operationId ?? string.Empty);
        }

        public async void TriggerJob(string name)
        {
            await Task.Yield();
            try
            {
                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        var response = client.GetAsync($"https://zetronjob.azurewebsites.net/api/zetronjobhttptrigger?name=" + name).Result;
                        var result = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }
    }
}
