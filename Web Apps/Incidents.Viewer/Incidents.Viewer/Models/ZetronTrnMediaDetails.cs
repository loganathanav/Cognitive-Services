using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Incidents.Viewer.Models
{
    public partial class ZetronTrnMediaDetails
    {
        public ZetronTrnMediaDetails()
        {
            ZetronTrnFrames = new HashSet<ZetronTrnFrames>();
        }

        public int MediaId { get; set; }
        public int IncidentId { get; set; }
        public string MediaUrl { get; set; }
        public string MediaSummaryUrl { get; set; }
        public int MediaType { get; set; }
        public DateTime PostedIon { get; set; }
        public string PostedBy { get; set; }
        public bool Status { get; set; }

        public ZetronMstIncidents Incident { get; set; }

        [JsonProperty(PropertyName = "frames")]
        public ICollection<ZetronTrnFrames> ZetronTrnFrames { get; set; }
    }
}
