using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Services.Analytics.Interfaces;
using Services.Analytics.Models;

namespace Services.Analytics.Mock
{
    public class DroneMock : IDrone
    {
         public DroneDetail GetCurrentLocationDetail()
         {
             return new DroneDetail()
             {
                 WindDirection = MockHelpers.RandomEnum<WindDirection>().GetDescription(),
                 Summary = MockHelpers.RandomEnum<Climate>().GetDescription(),
                 WindSpeed = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(0, 40)), 2),
                 Temperature = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(40, 42)), 2),
                 Humidity = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(20, 100)), 2),
                 DewPoint = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(20, 90)), 2),
                 Longitude = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(-180, 180)), 6),
                 Latitude = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(-90, 90)), 6),
                 AirQuality = Math.Round(Convert.ToDecimal(MockHelpers.RandomBetween(0, 500)), 2)
             };
         }

        

    }


}
