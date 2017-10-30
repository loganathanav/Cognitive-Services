using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Services.Analytics.Models
{
    public partial class ZetronTrnDroneLocations
    {
        public int LocationID { get; set; }
        public int MediaID { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public decimal WindSpeed { get; set; }
        public decimal DewPoint { get; set; }
        public string Summary { get; set; }
        public string WindDirection { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public ZetronTrnMediaDetails Media { get; set; }
    }
}