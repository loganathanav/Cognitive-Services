using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Analytics.Models
{
    public class DroneDetail
    {
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public decimal DewPoint { get; set; }
        public decimal WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public string Summary { get; set; }
        public decimal AirQuality { get; set; }
    }
}
