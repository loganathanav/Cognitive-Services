using Function.MediaQ.Analyzer;
using Function.MediaQ.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;

namespace Function.MediaQ
{
    public static class MediaQueue
    {
        [FunctionName("MediaQueue")]
        public static void Run([QueueTrigger("zetron-media", Connection = "AzureWebJobsStorage")]string myQueueItem, TraceWriter log)
        {
            var mediaJson = JObject.Parse(myQueueItem);
            log.Info($"Incoming JSON: {mediaJson.ToString()}");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-Key", ConfigurationManager.AppSettings["VisionAPIKey"]);
            string url = ConfigurationManager.AppSettings["VisionAPIHost"];

            var analysisInterval = TimeSpan.Parse(ConfigurationManager.AppSettings["AnalysisInterval"]);
            var analysisEntryCount = Convert.ToInt32(ConfigurationManager.AppSettings["AnalysisEntryCount"]);
            var connectionString = ConfigurationManager.ConnectionStrings["ZetronDb"].ConnectionString;
            var dbcontext = new ZetronDbContext(connectionString, log);
            var grapper = new FrameGrabber<FrameAnalysisResult>(mediaJson, dbcontext, log);
            var mediaAnalyzer = new MediaAnalyzer(mediaJson, client, grapper, dbcontext, log, analysisInterval, analysisEntryCount, url, (int)mediaJson["mediaId"]);
            mediaAnalyzer.ProcessMedia();

            log.Info($"MediaQueue trigger function completed for media id: {mediaJson["mediaId"]}");
        }
    }
}
