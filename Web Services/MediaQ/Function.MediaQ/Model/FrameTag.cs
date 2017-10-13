using System;

namespace Function.MediaQ.Model
{
    public partial class FrameTag
    {
        public int MediaId { get; set; }
        public DateTime FrameTime { get; set; }
        public string Tag { get; set; }
        public double ConfidenceLevel { get; set; }
    }
}
