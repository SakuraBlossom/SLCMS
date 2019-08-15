using System;
using System.Globalization;
using System.Windows.Data;

namespace SLCMS.View.Themes {

    public class BoolToYesNoConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null && (bool)value ? "YES" : "NO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null && (int)value == 0;
        }
    }
}
