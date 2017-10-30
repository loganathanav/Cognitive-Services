using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using Incidents.Viewer.Models;

namespace Incidents.Viewer.Controllers
{
    public class IncidentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _api;

        private ZetronMstIncidents incidents;
        public IncidentController(IConfiguration configuration)
        {
            _configuration = configuration;
            _api = _configuration.GetValue<string>("ZetronService");
        }

        public IActionResult Index()
        {
            incidents = new ZetronMstIncidents();
            using (HttpClientHandler httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    try
                    {
                        var response = client.GetAsync($"{_api}incidents/Active").Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            return View(incidents);
                        }
                        var allIncidents = JsonConvert.DeserializeObject<ZetronMstIncidents>(response.Content.ReadAsStringAsync().Result);
                        //var activeIncidents = allIncidents.Where(i => (i.Status != (int)IncidentStatus.Deactivated));
                        if (allIncidents != null)
                        {
                            incidents = allIncidents;
                        }

                    }
                    catch (Exception ex)
                    {
                        incidents.liveData = new Models.Live();
                        return View(incidents);
                    }

                }
            }
            incidents.liveData = new Models.Live();

            return View(incidents);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.Client, Duration = 6)]
        [HttpGet("live/{mediaId}")]
        public async Task<PartialViewResult> Live([FromRoute] int mediaId)
        {
            var liveData = new Live();
            using (HttpClientHandler httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    try
                    {
                        var response = client.GetAsync($"{_api}Medias/Analytics/{mediaId}").Result;


                        if (!response.IsSuccessStatusCode)
                        {
                            return PartialView("_live", liveData);
                        }
                        var tagdetails = JsonConvert.DeserializeObject<List<TagDetail>>(response.Content.ReadAsStringAsync().Result);
                        FireState outFireState;
                        SmokeState outSmokeState;
                        liveData.FireState = Enum.TryParse(tagdetails.FirstOrDefault(f => f.TagType == "Fire")?.Tag?.Split(' ')?[0],out outFireState) ? outFireState : (FireState)0;
                        liveData.SmokeState = Enum.TryParse(tagdetails.FirstOrDefault(f => f.TagType == "Smoke")?.Tag?.Split(' ')?[0], out outSmokeState) ? outSmokeState : (SmokeState)0;
                        var result = from t in tagdetails.Where(f => f.TagType == "Tag")
                                     select new TagCloud
                                     {
                                         text = t.Tag,
                                         weight = t.TagCount
                                     };
                        liveData.CloudTags = JsonConvert.SerializeObject(result.ToList());

                        var locationResponse = client.GetAsync($"{_api}location").Result;
                        liveData.Location = JsonConvert.DeserializeObject<LocationDetail>(locationResponse.Content.ReadAsStringAsync().Result);
                    }
                    catch (Exception ex)
                    {
                        return PartialView("_live", liveData);
                    }
                }
            }
            return PartialView("_live", liveData);
        }
    }
}