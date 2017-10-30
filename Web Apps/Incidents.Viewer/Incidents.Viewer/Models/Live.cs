using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incidents.Viewer.Models
{
    public class Live
    {
        public List<TagDetail> AllTags { get; set; }
        public string CloudTags { get; set; }
        public SmokeState SmokeState { get; set; }
        public FireState FireState { get; set; }
        public LocationDetail Location { get; set; } = new LocationDetail();
    }

    public class TagDetail
    {
        public Int64 Id { get; set; }
        public int TagCount { get; set; }
        public string Tag { get; set; }
        public string TagType { get; set; }
    }

    public class TagCloud
    {
        public string text { get; set; }
        public int weight { get; set; }
    }

    public class LocationDetail
    {
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public decimal DewPoint { get; set; }
        public decimal WindSpeed { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public string WindDirection { get; set; }
        public string Summary { get; set; }
        public decimal AirQuality { get; set; }
    }
}
