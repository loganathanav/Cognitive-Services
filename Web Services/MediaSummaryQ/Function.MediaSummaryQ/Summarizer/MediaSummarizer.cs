using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Function.MediaSummaryQ.Model;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.MediaServices.Client;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Auth;
namespace Function.MediaSummaryQ.Summarizer
{
    public class MediaSummarizer
    {
        private readonly JObject _mediaObject;
        private readonly ZetronDbContext _dbContext;
        private readonly TraceWriter _log;
        private readonly CloudMediaContext _context = null;
        private readonly StorageCredentials _blobCredentials;

        public MediaSummarizer(JObject mediaObject, ZetronDbContext dbContext, TraceWriter log,
            ITokenProvider credential, StorageCredentials blobCredentials)
        {
            _mediaObject = mediaObject;
            _dbContext = dbContext;
            _log = log;
            _context = new CloudMediaContext(credential);
            _blobCredentials = blobCredentials;
        }

        public void ProcessMedia()
        {
            // Run the thumbnail job.
            var asset = RunVideoThumbnailJob((string)_mediaObject["mediaName"], "config.json");
            if (asset != null)
            {
                Uri blobUri = new Uri(asset.Uri.ToString());
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = new Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer(blobUri, _blobCredentials);

                BlobContainerPermissions permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);
                var summaryVideoUrl = container.Uri.OriginalString + "/" + asset.AssetFiles.FirstOrDefault().Name;
                _log.Info($"Granted public access to {summaryVideoUrl}");

                _dbContext.UpdateSummaryUrl((int) _mediaObject["mediaId"], summaryVideoUrl);

            }
            else
            {
                _log.Info($"No assets found for media name: {_mediaObject["mediaName"]}");
            }
        }



        private IAsset RunVideoThumbnailJob(string inputMediaFile, string configurationFile)
        {

            _log.Info("Run Video started");
            // Create an asset and upload the input media file to storage.
            IAsset asset = CreateAssetAndUploadSingleFile(inputMediaFile, AssetCreationOptions.None);
            if (asset != null)
            {
                // Declare a new job.
                IJob job = _context.Jobs.Create("My Video Thumbnail Job");

                _log.Info("Job Created");
                // Get a reference to Azure Media Video Thumbnails.
                string MediaProcessorName = "Azure Media Video Thumbnails";

                var processor = GetLatestMediaProcessorByName(MediaProcessorName);

                //_log.Info("Read config file from " + Path.Combine(System.IO.Directory.GetParent(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath)).FullName, configurationFile));
                // Read configuration from the specified file.
                //  string configuration = "{\"version\": \"1.0\",\"options\": {\"outputAudio\": \"true\",\"maxMotionThumbnailDurationInSecs\": \"60\",\"fadeInFadeOut\": \"true\"}}";//        File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configurationFile)); //File.ReadAllText(configurationFile);            logWriter.Info("Configuration Read");

                string configuration = File.ReadAllText(Path.Combine(System.IO.Directory.GetParent(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath)).FullName, configurationFile));

                //_log.Info("Config " + configuration);
                // Create a task with the encoding details, using a string preset.
                ITask task = job.Tasks.AddNew("My Video Thumbnail Task",
                    processor,
                    configuration,
                    TaskOptions.None);

                // Specify the input asset.
                task.InputAssets.Add(asset);

                // Add an output asset to contain the results of the job.
                var outputAssetName = $"{inputMediaFile}-Summary";
                task.OutputAssets.AddNew(outputAssetName, AssetCreationOptions.None);

                _log.Info("Result file container created");

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
                    _log.Info(string.Format("Error: {0}. {1}",
                        error.Code,
                        error.Message));
                    return null;
                }

                if (job.State == JobState.Finished)
                {
                    _log.Info($"Asset creation completed for {outputAssetName}.");
                }

                return job.OutputMediaAssets[0];
            }
            return asset;
        }

        private IAsset CreateAssetAndUploadSingleFile(string assetName, AssetCreationOptions options)
        {

            IAsset asset = null;

            _log.Info("Entered Find File");
            try
            {
                if (_context.Assets.Count() > 0)
                {
                    _log.Info("Searching for asset");
                    //foreach (IAsset temp in _context.Assets)
                    //{
                    //    logWriter.Info(temp.Name);
                    //}
                    asset = _context.Assets.Where(x => x.Name == assetName).First();
                    _log.Info("File available");
                }
                else
                {
                    _log.Info("No files in Assets");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
            return asset;
        }


        private IMediaProcessor GetLatestMediaProcessorByName(string mediaProcessorName)
        {
            _log.Info("Entered Latest Media");
            var processor = _context.MediaProcessors
                .Where(p => p.Name == mediaProcessorName)
                .ToList()
                .OrderBy(p => new Version(p.Version))
                .LastOrDefault();

            if (processor == null)
            {
                _log.Info("Error in " + mediaProcessorName);
                throw new ArgumentException(string.Format("Unknown media processor",
                    mediaProcessorName));
            }

            _log.Info("Exit with Latest Media " + mediaProcessorName);
            return processor;
        }

        private void StateChanged(object sender, JobStateChangedEventArgs e)
        {
            _log.Info("Job state changed event:");
            _log.Info("  Previous state: " + e.PreviousState);
            _log.Info("  Current state: " + e.CurrentState);

            switch (e.CurrentState)
            {
                case JobState.Finished:
                    _log.Info("Job is finished.");
                    break;
                case JobState.Canceling:
                case JobState.Queued:
                case JobState.Scheduled:
                case JobState.Processing:
                    _log.Info("Please wait...\n");
                    break;
                case JobState.Canceled:
                case JobState.Error:
                    // Cast sender as a job.
                    IJob job = (IJob)sender;
                    // Display or log error details as needed.
                    // LogJobStop(job.Id);
                    _log.Info("Error Occured");
                    break;
                default:
                    break;
            }
        }

        private void BuildProgressiveDownloadURLs(IAsset asset)
        {
            // Create a 30-day readonly access policy. 
            IAccessPolicy policy = _context.AccessPolicies.Create("Streaming policy",
                TimeSpan.FromDays(30),
                AccessPermissions.Read);
            _log.Info("Policy created successfully");
            // Create an OnDemandOrigin locator to the asset. 
            ILocator originLocator = _context.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset,
                policy,
                DateTime.UtcNow.AddMinutes(-5));


            // Display some useful values based on the locator.
            _log.Info("Streaming asset base path on origin: ");
            _log.Info(originLocator.Path);


            // Get MP4 files.
            IEnumerable<IAssetFile> mp4AssetFiles = asset
                .AssetFiles
                .ToList()
                .Where(af => af.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));

            // Create a full URL to the MP4 files. Use this to progressively download files.
            foreach (var pd in mp4AssetFiles)
                _log.Info(originLocator.Path + pd.Name);
        }
    }
}
