using Function.MediaQ.Model;

namespace Function.MediaQ.Analyzer
{
    public class FrameAnalysisResult
    {
        public Microsoft.ProjectOxford.Face.Contract.Face[] Faces { get; set; } = null;
        public Microsoft.ProjectOxford.Common.Contract.EmotionScores[] EmotionScores { get; set; } = null;
        public string[] CelebrityNames { get; set; } = null;
        public PredictionTimes Tags { get; set; } = null;
    }
}
