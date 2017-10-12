using System;
using System.Collections.Generic;

namespace Services.Analytics.Models
{
    public partial class ZetronTrnFrameTags
    {
        public int FrameId { get; set; }
        public int MediaId { get; set; }
        public DateTime FrameTime { get; set; }
        public string Tag { get; set; }
        public int ConfidenceLevel { get; set; }

        public ZetronTrnMediaDetails Media { get; set; }
    }
}
