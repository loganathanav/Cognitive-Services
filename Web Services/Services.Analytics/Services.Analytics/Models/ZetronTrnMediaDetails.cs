using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Services.Analytics.Models
{
    public partial class ZetronTrnMediaDetails
    {
        public ZetronTrnMediaDetails()
        {
            ZetronTrnFrameTags = new HashSet<ZetronTrnFrameTags>();
        }

        public int MediaId { get; set; }
        public int IncidentId { get; set; }
        public string MediaUrl { get; set; }
        public int MediaType { get; set; }
        public DateTime PostedIon { get; set; }
        public string PostedBy { get; set; }
        public bool Status { get; set; }

        public ZetronMstIncidents Incident { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public ICollection<ZetronTrnFrameTags> ZetronTrnFrameTags { get; set; }
    }
}
