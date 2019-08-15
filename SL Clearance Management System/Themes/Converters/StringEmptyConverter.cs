using System;
using System.Windows.Data;
using System.Globalization;

namespace SLCMS.View.Themes
{
    public class StringEmptyOrWhiteSpacetoBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace((string)value);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
