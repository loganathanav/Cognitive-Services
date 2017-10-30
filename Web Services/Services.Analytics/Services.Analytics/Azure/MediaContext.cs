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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Services.Analytics.Interfaces;

namespace Services.Analytics
{
    public class MediaContext
    {
        //private readonly ZetronContext _dbContext;
        private readonly AppSettings _appSettings;

        private readonly CloudMediaContext _mediaContext;
        private readonly MediaServicesCredentials _cachedCredentials;
        private readonly IChannel _channel;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudQueueClient _queueClient;

        private IConfiguration _configuration { get; }
        private DbContextOptionsBuilder<ZetronContext> options;
        //private readonly IDrone _drone;

        public MediaContext(IOptions<AppSettings> appSettings, IConfiguration configuration)//, IDrone drone)
        {
            _appSettings = appSettings.Value;
            _configuration = configuration;
            //_drone = drone;

            options = new DbContextOptionsBuilder<ZetronContext>();
            options.UseSqlServer(_configuration["ZetronDb"]);

            try
            {
                var tokenCredentials = new AzureAdTokenCredentials("ilink-systems.com", new AzureAdClientSymmetricKey("44e26513-dcb2-4c96-b841-21b6a262ec87", "YcFIlPERjGt4yGi8/ntOo8mEgqqPLkH7keWNk82/Iss="), AzureEnvironments.AzureCloudEnvironment);
                var tokenProvider = new AzureAdTokenProvider(tokenCredentials);
                _mediaContext = new CloudMediaContext(new Uri(@"https://zetronpoc.restv2.westcentralus-2.media.azure.net/api/"), tokenProvider);

                _channel = _mediaContext.Channels.FirstOrDefault();
                _storageAccount = CloudStorageAccount.Parse(_appSettings.StorageAccountConnection);
                _queueClient = _storageAccount.CreateCloudQueueClient();
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

        public void StopAzureProcess(int incidentId, IncidentStatus incidentStatus)
        {
            try
            {
                using (var _dbContext = new ZetronContext(options.Options))
                {
                    //Trigger Media Summary Q
                    if (incidentStatus == IncidentStatus.Stopped)
                    {
                        var media = _dbContext.ZetronTrnMediaDetails.FirstOrDefault(i => i.IncidentId == incidentId);
                        if (media != null)
                        {
                            var queue = _queueClient.GetQueueReference("zetron-media-summary");
                            queue.CreateIfNotExistsAsync();
                            var queueMessage = new JObject
                            {
                                ["mediaId"] = media.MediaId,
                                ["mediaUrl"] = media.MediaUrl,
                                ["mediaName"] = media.Name
                            };

                            var message = new CloudQueueMessage(queueMessage.ToString());
                            queue.AddMessageAsync(message);
                        }
                        else
                        {
                            Log("MediaContext - StopAzureProcess - No matching media details found!");
                        }
                    }

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
                    Task prog = program.StartAsync();
                    Task stream = UpdateStreamingUrl(asset, originLocator, zetronMstIncidents);

                    Task.WaitAll(prog, stream);
                    if ((zetronMstIncidents.Status == (int)IncidentStatus.Started) || (zetronMstIncidents.Status == (int)IncidentStatus.Processing))
                    {
                        TriggerJob(zetronMstIncidents.IncidentId);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        public void DeleteAllAssets()
        {
            try
            {
                _mediaContext.Assets.ToList().ForEach(p => p.DeleteAsync());
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        private async Task UpdateStreamingUrl(IAsset asset, ILocator originLocator, ZetronMstIncidents zetronMstIncidents)
        {
            try
            {
                //Get manifest file to create streaming url
                IAssetFile manifestFile = asset.AssetFiles.Where(f => f.Name.ToLower().EndsWith(".ism")).FirstOrDefault();
                string streamUrl = originLocator.Path + manifestFile.Name + "/manifest(format=m3u8-aapl)";
                //var droneDetail = _drone.GetCurrentLocationDetail();
                ZetronTrnMediaDetails mediaDetails = new ZetronTrnMediaDetails()
                {
                    IncidentId = zetronMstIncidents.IncidentId,
                    MediaUrl = streamUrl,
                    Name = asset.Name,
                    MediaType = 1,
                    PostedIon = DateTime.Now.ToUniversalTime(),
                    PostedBy = "Admin",
                    Status = true
                };
                using (var _dbContext = new ZetronContext(options.Options))
                {
                    //var mediaId = _dbContext.ZetronTrnMediaDetails.Max(m => m.MediaId) + 1;
                    _dbContext.ZetronTrnMediaDetails.Add(mediaDetails);

                    ZetronMstIncidents recordtoUpdate = null;
                    if (zetronMstIncidents.Status == (int)IncidentStatus.Started)
                    {
                        recordtoUpdate = _dbContext.ZetronMstIncidents.Single(i => i.IncidentId == zetronMstIncidents.IncidentId);
                        recordtoUpdate.Status = (int)IncidentStatus.Processing;
                    }

                    //_dbContext.ZetronTrnDroneLocations.Add(new ZetronTrnDroneLocations()
                    //{
                    //    MediaID = mediaId,
                    //    DewPoint = droneDetail.DewPoint,
                    //    Humidity = droneDetail.Humidity,
                    //    Temperature = droneDetail.Temperature,
                    //    WindSpeed = droneDetail.WindSpeed,
                    //    Summary = droneDetail.Summary,
                    //    WindDirection = droneDetail.WindDirection,
                    //    Longitude = droneDetail.Longitude,
                    //    Latitude = droneDetail.Latitude
                    //});

                    await _dbContext.SaveChangesAsync();
                }
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

        public void TriggerJob(int incidentId = 0)
        {
            //await Task.Yield();
            try
            {
                using (var _dbContext = new ZetronContext(options.Options))
                {
                    var media = _dbContext.ZetronTrnMediaDetails.FirstOrDefault(i => i.IncidentId == incidentId);
                    if (media != null)
                    {
                        //_storageAccount = CloudStorageAccount.Parse(_appSettings.StorageAccountConnection);
                        //_queueClient = _storageAccount.CreateCloudQueueClient();

                        //Remove tags
                        var frames = _dbContext.ZetronTrnFrames.Where(f => f.MediaId == media.MediaId);
                        if (frames != null && frames.Count() > 0)
                        {
                            foreach (var frame in frames)
                            {
                                var tags = _dbContext.ZetronTrnFrameTags.Where(t => t.FrameId == frame.FrameId);
                                if (tags != null && tags.Count() > 0)
                                {
                                    _dbContext.ZetronTrnFrameTags.RemoveRange(tags);
                                }
                            }
                            _dbContext.ZetronTrnFrames.RemoveRange(frames);
                            Log($"MediaContext - TriggerJob - Media id {media.MediaId} related tags removed!");
                            _dbContext.SaveChanges();
                        }

                        var queue = _queueClient.GetQueueReference("zetron-media");
                        queue.CreateIfNotExistsAsync();
                        var queueMessage = new JObject
                        {
                            ["incidentId"] = incidentId,
                            ["mediaId"] = media.MediaId,
                            ["mediaUrl"] = media.MediaUrl
                        };

                        var message = new CloudQueueMessage(queueMessage.ToString());
                        queue.AddMessageAsync(message);
                    }
                    else
                    {
                        Log("MediaContext - TriggerJob - No matching media details found!");
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
