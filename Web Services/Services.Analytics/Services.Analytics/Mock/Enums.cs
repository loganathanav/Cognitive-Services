using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Analytics.Mock
{
    public enum WindDirection
    {
        [Description("North")]
        N,
        [Description("North East")]
        NE,
        [Description("East")]
        E,
        [Description("South East")]
        SE,
        [Description("South")]
        S,
        [Description("South West")]
        SW,
        [Description("West")]
        W,
        [Description("North West")]
        NW
    }

    public enum Climate
    {
        [Description("Cloudy")]
        Cloudy,
        [Description("Partially cloud")]
        PartiallyCloud,
        [Description("Clear")]
        Clear,
        [Description("Sunny")]
        Sunny,
        [Description("Raining")]
        Raining,
        [Description("Drizzling")]
        Drizzling,
        [Description("Partially sunny")]
        PartiallySunny,
        [Description("Heavy raining")]
        HeavyRaining,
    }
}
