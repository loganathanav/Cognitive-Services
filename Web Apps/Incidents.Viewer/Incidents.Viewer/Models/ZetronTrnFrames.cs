using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incidents.Viewer.Models
{
    public class ZetronTrnFrames
    {
        public ZetronTrnFrames()
        {
            ZetronTrnFrameTags = new HashSet<ZetronTrnFrameTags>();
        }

        public int FrameId { get; set; }
        public int MediaId { get; set; }
        public DateTime FrameTime { get; set; }

        public ZetronTrnMediaDetails Media { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public ICollection<ZetronTrnFrameTags> ZetronTrnFrameTags { get; set; }
    }
}
