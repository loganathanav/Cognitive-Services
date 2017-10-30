using System;
using System.Collections.Generic;

namespace Incidents.Viewer.Models
{
    public partial class ZetronTrnFrameTags
    {
        public int TagId { get; set; }
        public int FrameId { get; set; }
        public string Tag { get; set; }
        public float ConfidenceLevel { get; set; }

        public ZetronTrnFrames Frame { get; set; }
    }
}
