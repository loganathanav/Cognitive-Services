﻿using Function.MediaQ.Model;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Function.MediaQ.Analyzer
{
    public class MediaAnalyzer
    {
        private readonly HttpClient _visionClient = null;
        private readonly string _customVisionUrl = null;
        private readonly JObject _mediaObject;
        private readonly FrameGrabber<FrameAnalysisResult> _grabber = null;
        private readonly ZetronDbContext _dbContext;
        private readonly TraceWriter _log;
        private readonly TimeSpan _analysisInterval;
        private static readonly ImageEncodingParam[] s_jpegParams =
        {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 60)
        };

        private List<FrameTag> Tags = new List<FrameTag>();
        private int PushCount = 0;
        private int MediaId;

        public MediaAnalyzer(JObject mediaObject, HttpClient visionClient,
            FrameGrabber<FrameAnalysisResult> grapper, ZetronDbContext dbContext, TraceWriter log, TimeSpan analysisInterval, string customVisionUrl)
        {
            _mediaObject = mediaObject;
            _visionClient = visionClient;
            _grabber = grapper;
            _dbContext = dbContext;
            _log = log;
            _analysisInterval = analysisInterval;
            _customVisionUrl = customVisionUrl;

            MediaId = (int)_mediaObject["mediaId"];

            _grabber.NewResultAvailable += (s, e) =>
            {
                if (e.TimedOut)
                {
                    _log.Info($"Timeout occured!");
                    if (Tags.Count > 0)
                        AddTagsAndCheckEventStatus();
                    else
                        CheckLiveEventStatus();
                }
                else if (e.Exception != null)
                {
                    _log.Info($"Inside NewResultAvailable event exception");
                    string message = e.Exception.Message;
                    var visionEx = e.Exception as HttpResponseException;// Microsoft.ProjectOxford.Vision.ClientException;
                    _log.Error(message, visionEx);

                    if (e.Exception.ToString().Contains("Empty JPEG image"))
                    {
                        if (Tags.Count > 0)
                            AddTagsAndCheckEventStatus();
                        else
                            CheckLiveEventStatus();
                    }
                }
                else
                {
                    var resultTags = e.Analysis?.Tags;
                    if (resultTags?.Predictions != null && resultTags.Predictions.Any())
                    {
                        foreach (var tag in resultTags.Predictions)
                        {
                            if (decimal.Round((decimal)tag.Probability, 8) > 0)
                            {
                                Tags.Add(new FrameTag()
                                {
                                    MediaId = MediaId,
                                    FrameTime = resultTags.Created,
                                    Tag = tag.Tag,
                                    ConfidenceLevel = tag.Probability
                                });
                            }
                        }
                        PushCount++;
                    }
                    else
                    {
                    }

                    if (PushCount >= 5 && Tags.Count > 0)
                    {
                        AddTagsAndCheckEventStatus();
                    }

                }
            };

            _grabber.ProcessingStopped += (s, e) =>
            {
                if (Tags.Count <= 0) return;
                _log.Info($"Video process stopped., Adding remaining tags.");
                AddTagsAndCheckEventStatus();
            };
        }

        private async void AddTagsAndCheckEventStatus()
        {
            _dbContext.AddTags(Tags);
            Tags.Clear();
            PushCount = 0;

            await CheckLiveEventStatus();
        }

        private async Task CheckLiveEventStatus()
        {
            var incidentStatus = _dbContext.CheckEventState(MediaId);
            if (incidentStatus == IncidentStatus.Stopped || incidentStatus == IncidentStatus.Deactivated)
            {
                _log.Info($"Completing media analysis because incident status changed to {incidentStatus}.");
                await _grabber.StopProcessingAsync();
            }
        }

        public async Task ProcessMedia()
        {
            _grabber.AnalysisFunction = TaggingAnalysisFunction;
            _grabber.TriggerAnalysisOnInterval(_analysisInterval);

            if (_grabber.CheckStreamingStatus())
                await _grabber.StartProcessingAsync();
        }

        private async Task<FrameAnalysisResult> TaggingAnalysisFunction(VideoFrame frame)
        {
            PredictionTimes predictions;
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);

            using (var content = new ByteArrayContent(jpg.ToArray()))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var analysis = await _visionClient.PostAsync(_customVisionUrl, content);

                predictions = analysis.Content.ReadAsAsync<PredictionTimes>().Result;
            }
            return new FrameAnalysisResult() { Tags = predictions };
        }
    }
}
