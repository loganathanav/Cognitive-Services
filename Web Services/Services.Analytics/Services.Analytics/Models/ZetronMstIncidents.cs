using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Services.Analytics.Models
{
    public partial class ZetronMstIncidents
    {
        public ZetronMstIncidents()
        {
            ZetronTrnMediaDetails = new HashSet<ZetronTrnMediaDetails>();
        }

        public int IncidentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReportedOn { get; set; }
        public string Location { get; set; }
        public int Status { get; set; }

        [JsonProperty(PropertyName = "medias")]
        public ICollection<ZetronTrnMediaDetails> ZetronTrnMediaDetails { get; set; }
    }
}
