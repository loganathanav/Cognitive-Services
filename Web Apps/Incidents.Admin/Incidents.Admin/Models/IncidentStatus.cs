using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incidents.Admin.Models
{
    public enum IncidentStatus
    {
        Initiated = 1,
        Started = 2,
        Processing = 3,
        Stopped = 4,
        Deactivated = 5,
    }
}
