using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Services.Analytics.Mock
{
    public static class MockHelpers
    {
        private static readonly Random rnd = new Random();
        public static string GetDescription(this Enum sourcEnum)
        {
            Type type = sourcEnum.GetType();
            string name = Enum.GetName(type, sourcEnum);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                        Attribute.GetCustomAttribute(field,
                            typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
        public static T RandomEnum<T>()
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(new Random().Next(values.Length));
        }
        public static double RandomBetween(double minimum, double maximum)
        {
            var next = rnd.NextDouble();
            return minimum + (next * (maximum - minimum));
        }
    }
}
