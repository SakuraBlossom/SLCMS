using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SLCMS.View.Themes {
    public class TabIndexConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var tabItem       = value as TabItem;
            var fromContainer = ItemsControl.ItemsControlFromItemContainer(tabItem).ItemContainerGenerator;

            var items = fromContainer.Items.Cast<TabItem>().Where(x => x.Visibility == Visibility.Visible).ToList();
            var count = items.Count;

            var index = items.IndexOf(tabItem);
            if(index == 0)
                return "First";

            return count - 1 == index ? "Last" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}
