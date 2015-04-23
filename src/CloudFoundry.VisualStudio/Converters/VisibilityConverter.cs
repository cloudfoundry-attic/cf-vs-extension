using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CloudFoundry.VisualStudio.Converters
{
    public class VisiblityConverter : IValueConverter
    {
        public bool Reversed { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           if(value as bool? != Reversed) {
               return Visibility.Visible;
           }
           else
           {
               return Visibility.Hidden;
           }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("VisiblityConverter can only be used OneWay.");
        }
    }
}
