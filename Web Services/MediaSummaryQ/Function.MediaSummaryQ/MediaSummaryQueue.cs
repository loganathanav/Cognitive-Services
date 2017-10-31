using Function.MediaSummaryQ.Model;
using Function.MediaSummaryQ.Summarizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace Function.MediaSummaryQ
{
    public static class MediaSummaryQueue
    {
        [FunctionName("MediaSummaryQueue")]
        public static void Run([QueueTrigger("zetron-media-summary", Connection = "AzureWebJobsStorage")]string myQueueItem, TraceWriter log)
        {
            var mediaJson = JObject.Parse(myQueueItem);
            log.Info($"Video Summary Queue processing started for media: {mediaJson["mediaId"]}");

            var connectionString = ConfigurationManager.ConnectionStrings["ZetronDb"].ConnectionString;
            var mediaServiceCredential = new MediaServicesCredentials(ConfigurationManager.AppSettings["MediaClientId"],
                ConfigurationManager.AppSettings["MediaClientKey"]);
            var blobCredentials = new StorageCredentials(ConfigurationManager.AppSettings["StorageAccount"],
                ConfigurationManager.AppSettings["StorageAccountKey"]);
            var dbcontext = new ZetronDbContext(connectionString, log);

            var mediaSummarizer = new MediaSummarizer(mediaJson, dbcontext, log, mediaServiceCredential,
                blobCredentials);
            mediaSummarizer.ProcessMedia();
        }

    }
}
