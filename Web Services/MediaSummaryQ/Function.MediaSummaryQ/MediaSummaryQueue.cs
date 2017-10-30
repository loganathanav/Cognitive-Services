using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Function.MediaSummaryQ
{
    public static class MediaSummaryQueue
    {
        private static CloudMediaContext _context = null;
        private static TraceWriter logWriter = null;

        [FunctionName("MediaSummaryQueue")]
        public static void Run([QueueTrigger("zetron-media-summary", Connection = "AzureWebJobsStorage")]string myQueueItem, TraceWriter log)
        {
            var mediaJson = JObject.Parse(myQueueItem);
            logWriter = log;
            log.Info($"Video Summary Queue processing started with : {mediaJson}");

            MediaServicesCredentials credential = new MediaServicesCredentials(ConfigurationManager.AppSettings["MediaClientId"], ConfigurationManager.AppSettings["MediaClientKey"]);
            _context = new CloudMediaContext(credential);


            // Run the thumbnail job.
            var asset = RunVideoThumbnailJob((string)mediaJson["mediaName"], "config.json");
            if (asset != null)
            {
                Microsoft.WindowsAzure.Storage.Auth.StorageCredentials blobCredentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(ConfigurationManager.AppSettings["StorageAccount"], ConfigurationManager.AppSettings["StorageAccountKey"]);
                Uri blobUri = new Uri(asset.Uri.ToString());
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = new Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer(blobUri, blobCredentials);

                BlobContainerPermissions permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);

                logWriter.Info(container.Uri.OriginalString + "\\" + asset.AssetFiles.FirstOrDefault().Name);
            }
            else
            {
                logWriter.Info($"No assets found for media name: {mediaJson["mediaName"]}");
            }
            //log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }

        static IAsset RunVideoThumbnailJob(string inputMediaFile, string configurationFile)
        {

            logWriter.Info("Run Video started");
            // Create an asset and upload the input media file to storage.
            IAsset asset = CreateAssetAndUploadSingleFile(inputMediaFile, AssetCreationOptions.None);
            if (asset != null)
            {
                // Declare a new job.
                IJob job = _context.Jobs.Create("My Video Thumbnail Job");

                logWriter.Info("Job Created");
                // Get a reference to Azure Media Video Thumbnails.
                string MediaProcessorName = "Azure Media Video Thumbnails";

                var processor = GetLatestMediaProcessorByName(MediaProcessorName);

                logWriter.Info("Read config file from " + Path.Combine(System.IO.Directory.GetParent(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath)).FullName, configurationFile));
                // Read configuration from the specified file.
                //  string configuration = "{\"version\": \"1.0\",\"options\": {\"outputAudio\": \"true\",\"maxMotionThumbnailDurationInSecs\": \"60\",\"fadeInFadeOut\": \"true\"}}";//        File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configurationFile)); //File.ReadAllText(configurationFile);            logWriter.Info("Configuration Read");

                string configuration = File.ReadAllText(Path.Combine(System.IO.Directory.GetParent(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath)).FullName, configurationFile));

                logWriter.Info("Config " + configuration);
                // Create a task with the encoding details, using a string preset.
                ITask task = job.Tasks.AddNew("My Video Thumbnail Task",
                    processor,
                    configuration,
                    TaskOptions.None);

                // Specify the input asset.
                task.InputAssets.Add(asset);

                // Add an output asset to contain the results of the job.
                task.OutputAssets.AddNew(inputMediaFile + "-Summary", AssetCreationOptions.None);

                logWriter.Info("Result file uploaded");

                // Use the following event handler to check job progress.  
                job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

                // Launch the job.
                job.Submit();

                // Check job execution and wait for job to finish.
                Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);

                progressJobTask.Wait();

                // If job state is Error, the event handling
                // method for job progress should log errors.  Here we check
                // for error state and exit if needed.
                if (job.State == JobState.Error)
                {
                    ErrorDetail error = job.Tasks.First().ErrorDetails.First();
                    logWriter.Info(string.Format("Error: {0}. {1}",
                                                    error.Code,
                                                    error.Message));
                    return null;
                }

                return job.OutputMediaAssets[0];
            }
            return asset;
        }

        static IAsset CreateAssetAndUploadSingleFile(string assetName, AssetCreationOptions options)
        {

            IAsset asset = null;

            logWriter.Info("Entered Find File");
            try
            {
                if (_context.Assets.Count() > 0)
                {
                    logWriter.Info("Searching for asset");
                    //foreach (IAsset temp in _context.Assets)
                    //{
                    //    logWriter.Info(temp.Name);
                    //}
                    asset = _context.Assets.Where(x => x.Name == assetName).First();
                    logWriter.Info("File available");
                }
                else
                {
                    logWriter.Info("No files in Assets");
                }
            }
            catch (Exception ex)
            {
                logWriter.Error(ex.Message, ex);
            }
            return asset;
        }


        static IMediaProcessor GetLatestMediaProcessorByName(string mediaProcessorName)
        {
            logWriter.Info("Entered Latest Media");
            var processor = _context.MediaProcessors
                .Where(p => p.Name == mediaProcessorName)
                .ToList()
                .OrderBy(p => new Version(p.Version))
                .LastOrDefault();

            if (processor == null)
            {
                logWriter.Info("Error in " + mediaProcessorName);
                throw new ArgumentException(string.Format("Unknown media processor",
                                                           mediaProcessorName));
            }

            logWriter.Info("Exit with Latest Media " + mediaProcessorName);
            return processor;
        }

        static private void StateChanged(object sender, JobStateChangedEventArgs e)
        {
            logWriter.Info("Job state changed event:");
            logWriter.Info("  Previous state: " + e.PreviousState);
            logWriter.Info("  Current state: " + e.CurrentState);

            switch (e.CurrentState)
            {
                case JobState.Finished:
                    logWriter.Info("Job is finished.");
                    break;
                case JobState.Canceling:
                case JobState.Queued:
                case JobState.Scheduled:
                case JobState.Processing:
                    logWriter.Info("Please wait...\n");
                    break;
                case JobState.Canceled:
                case JobState.Error:
                    // Cast sender as a job.
                    IJob job = (IJob)sender;
                    // Display or log error details as needed.
                    // LogJobStop(job.Id);
                    logWriter.Info("Error Occured");
                    break;
                default:
                    break;
            }
        }

        private static void BuildProgressiveDownloadURLs(IAsset asset)
        {
            // Create a 30-day readonly access policy. 
            IAccessPolicy policy = _context.AccessPolicies.Create("Streaming policy",
                TimeSpan.FromDays(30),
                AccessPermissions.Read);
            logWriter.Info("Policy created successfully");
            // Create an OnDemandOrigin locator to the asset. 
            ILocator originLocator = _context.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset,
                policy,
                DateTime.UtcNow.AddMinutes(-5));


            // Display some useful values based on the locator.
            logWriter.Info("Streaming asset base path on origin: ");
            logWriter.Info(originLocator.Path);


            // Get MP4 files.
            IEnumerable<IAssetFile> mp4AssetFiles = asset
                .AssetFiles
                .ToList()
                .Where(af => af.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));

            // Create a full URL to the MP4 files. Use this to progressively download files.
            foreach (var pd in mp4AssetFiles)
                logWriter.Info(originLocator.Path + pd.Name);
        }
    }
}
