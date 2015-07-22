namespace CloudFoundry.VisualStudio.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;

    public class XmlUriToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            XmlUri input = value as XmlUri;
            if (input == null)
            {
                return string.Empty;
            }
            else
            {
                return input.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input = value as string;
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            else
            {
                return new XmlUri(new Uri(input, UriKind.Absolute));
            }
        }
    }
}
