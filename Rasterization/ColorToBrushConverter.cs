using System;
using System.Globalization ;
using System.Windows.Data ;
using System.Windows.Media ;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Rasterization
{
    class ColorToBrushConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                List<Color> ColorInformations = new List<Color>();
                var properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
                ColorInformations = properties.Select(prop =>
                {
                    return (Color)prop.GetValue(null, null);
                }).ToList();

                int index;
                if ((index = ColorInformations.FindIndex(c => c == ((SolidColorBrush)value).Color)) != -1)
                {
                    return index;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}