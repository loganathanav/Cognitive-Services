using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Analytics.Models
{
    public class AppSettings
    {
        public string MediaServicesAccountName { get; set; }
        public string MediaServicesAccountKey { get; set; }
        public string StorageAccountConnection { get; set; }
    }
}
