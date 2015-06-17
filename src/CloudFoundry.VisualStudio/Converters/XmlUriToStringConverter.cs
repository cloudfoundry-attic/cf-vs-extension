using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CloudFoundry.VisualStudio.Converters
{
    public class XmlUriToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            XmlUri input = value as XmlUri;
            if (input == null) return String.Empty;
            else return input.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input = value as string;
            if (String.IsNullOrEmpty(input)) return null;
            else return new XmlUri(new Uri(input, UriKind.Absolute));
        }
    }
}
