using System;
using System.Globalization;
using System.Windows.Data;

namespace SLCMS.View.Themes
{
    public class NullToBooleanFalseConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new Exception("NullToBooleanFalseConvertor: Only one way conversion");
        }
    }
}
