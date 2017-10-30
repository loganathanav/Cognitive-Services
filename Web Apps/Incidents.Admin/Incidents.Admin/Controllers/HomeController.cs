using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Incidents.Admin.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Incidents.Admin.Controllers
{

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _api;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _api = _configuration.GetValue<string>("ZetronService");
        }

        public async Task<ActionResult> List()
        {
            var incidents = new List<Incident>();
            using (HttpClientHandler httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    try
                    {
                        var response = client.GetAsync($"{_api}incidents").Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            return View(incidents);
                        }
                        var allIncidents = JsonConvert.DeserializeObject<List<Incident>>(response.Content.ReadAsStringAsync().Result);
                        var activeIncidents = allIncidents.Where(i => (i.Status != (int)IncidentStatus.Deactivated));
                        if (activeIncidents != null && activeIncidents.Count() > 0)
                        {
                            incidents = activeIncidents.ToList();
                        }

                    }
                    catch (Exception ex)
                    {
                        return View(incidents);
                    }

                }
            }

            //// Mock Data
            //incidents.Add(new Incident() { IncidentId = 6, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 1 });
            //incidents.Add(new Incident() { IncidentId = 1, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 2, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 3, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 4, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 5, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });

            return View(incidents);
        }
        public async Task<ActionResult> Index()
        {
            var incidents = new List<Incident>();
            using (HttpClientHandler httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    try
                    {
                        var response = client.GetAsync($"{_api}incidents").Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            return View(incidents);
                        }
                        var allIncidents = JsonConvert.DeserializeObject<List<Incident>>(response.Content.ReadAsStringAsync().Result);
                        var activeIncidents = allIncidents.Where(i => (i.Status != (int)IncidentStatus.Deactivated));
                        if (activeIncidents != null && activeIncidents.Count() > 0)
                        {
                            incidents = activeIncidents.ToList();
                        }

                    }
                    catch (Exception ex)
                    {
                        return View(incidents);
                    }

                }
            }

            //// Mock Data
            //incidents.Add(new Incident() { IncidentId = 6, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 1 });
            //incidents.Add(new Incident() { IncidentId = 1, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 2, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 3, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 4, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });
            //incidents.Add(new Incident() { IncidentId = 5, Description = "desc", Title = "Fire at Velachery Chennai Silks", Location = "4401 4th Ave S, Seattle, WA 98134", ReportedOn = DateTime.Now, Status = 3 });

            return View(incidents);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost("create")]
        public async Task<ActionResult> Create(Incident incident)
        {
            try
            {
                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        var incidentRequest = JObject.FromObject(new
                        {
                            Title = incident.Title,
                            Description = incident.Description,
                            ReportedOn = DateTime.Now.ToUniversalTime(),
                            Location = incident.Location,
                            Status = incident.IsImmediateProcessing ? IncidentStatus.Started : IncidentStatus.Initiated
                        });

                        var response = client.PostAsync($"{_api}incidents", new StringContent(incidentRequest.ToString(), Encoding.UTF8, "application/json")).Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest();
                        }
                        var result = response.Content.ReadAsStringAsync().Result;

                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(Incident incident)
        {
            try
            {
                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        var incidentRequest = JObject.FromObject(new
                        {
                            Title = incident.Title,
                            Description = incident.Description,
                            ReportedOn = incident.ReportedOn,
                            Location = incident.Location,
                            Status = incident.Status,
                            IncidentId = incident.IncidentId
                        });
                        var response = client.PutAsync($"{_api}incidents/{incident.IncidentId}", new StringContent(incidentRequest.ToString(), Encoding.UTF8, "application/json")).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest();
                        }
                        var result = response.Content.ReadAsStringAsync().Result;
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost("trigger")]
        public async Task<ActionResult> Trigger(Incident incident)
        {
            try
            {
                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        var incidentRequest = JObject.FromObject(new
                        {
                            Title = incident.Title,
                            Description = incident.Description,
                            ReportedOn = incident.ReportedOn,
                            Location = incident.Location,
                            Status = incident.Status,
                            IncidentId = incident.IncidentId
                        });
                        var response = client.PutAsync($"{_api}incidents/{incident.IncidentId}", new StringContent(incidentRequest.ToString(), Encoding.UTF8, "application/json")).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest();
                        }
                        var result = response.Content.ReadAsStringAsync().Result;
                    }
                }
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.Client, Duration = 10)]
        [HttpGet("live")]
        public async Task<PartialViewResult> Live()
        {
            var incidents = new List<Incident>();
            using (HttpClientHandler httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    try
                    {
                        var response = client.GetAsync($"{_api}incidents").Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            return PartialView("_live", incidents);
                        }
                        var allIncidents = JsonConvert.DeserializeObject<List<Incident>>(response.Content.ReadAsStringAsync().Result);
                        var activeIncidents = allIncidents.Where(i => (i.Status != (int)IncidentStatus.Deactivated && i.Status != (int)IncidentStatus.Stopped));
                        if (activeIncidents != null && activeIncidents.Count() > 0)
                        {
                            incidents = activeIncidents.ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        return PartialView("_live", incidents);
                    }

                }
            }
            return PartialView("_live", incidents);
        }
    }
}
