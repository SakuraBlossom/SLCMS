using System;
using System.Windows.Data;

namespace SLCMS.View.Themes
{
    public class DataGridSearchValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var cellText = values[0]?.ToString() ?? string.Empty;
            var searchText = (values[1] as string)?.ToUpper();

            if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(cellText) && cellText != "NA")
            {
                return cellText.Contains(searchText);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
