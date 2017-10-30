using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Services.Analytics.Models;

namespace Services.Analytics.Interfaces
{
    public interface IDrone
    {
        DroneDetail GetCurrentLocationDetail();
    }
}



