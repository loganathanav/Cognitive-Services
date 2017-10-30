using System;
using System.Collections.Generic;

namespace Services.Analytics.Models
{
    public partial class ZetronTrnFrameTags
    {
        public int TagId { get; set; }
        public int FrameId { get; set; }
        public string Tag { get; set; }
        public decimal ConfidenceLevel { get; set; }

        public ZetronTrnFrames Frame { get; set; }
    }
}
