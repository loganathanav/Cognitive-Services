using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incidents.Admin.Models
{
    public class Incident
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int IncidentId { get; set; }
        public DateTime ReportedOn { get; set; }
        public int Status { get; set; }
        public bool IsImmediateProcessing { get; set; }
    }
}
