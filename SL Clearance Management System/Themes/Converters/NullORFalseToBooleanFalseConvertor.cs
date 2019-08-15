using System;
using System.Globalization;
using System.Windows.Data;

namespace SLCMS.View.Themes
{
    public class NullOrFalseToBooleanFalseConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) || (value is int i && i != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("NullOrFalseToBooleanFalseConvertor: Only one way conversion");
        }
    }
}
