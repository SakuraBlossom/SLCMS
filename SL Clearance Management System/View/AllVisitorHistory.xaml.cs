using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS.View {
    /// <summary>
    /// Interaction logic for AllVisitorHistory.xaml
    /// </summary>
    public partial class AllVisitorHistory {

        public AllVisitorHistoryViewModel MyViewModel;

        public AllVisitorHistory() {
            InitializeComponent();
            DataContext = MyViewModel = new AllVisitorHistoryViewModel(SearchTextBox, StartSearchDateEntry, EndSearchDateEntry);
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var textToMatch = SearchTextBox?.Text;
            var cv          = CollectionViewSource.GetDefaultView(VisitorHistoryDataGrid.ItemsSource);

            if (string.IsNullOrEmpty(textToMatch)|| textToMatch == "NA" || !((VisitorHistoryDataGrid.ItemsSource) is IList<VisitorRecord>))
            {
                cv.Filter = null;
                return;
            }

            VisitorHistoryDataGrid.SelectedItems.Clear();
            cv.Filter = arg => {
                if(!(arg is VisitorRecord x))
                    return false;

                if (x.NRIC != textToMatch
                    && x.EscortNRIC != textToMatch
                    && x.PersonnelPass != textToMatch
                    && x.VehiclePass != textToMatch
                    && x.LockerNum != textToMatch)
                    return (x.NRIC.Contains(textToMatch)
                            || x.EscortNRIC.Contains(textToMatch)
                            || x.PersonnelPass.Contains(textToMatch)
                            || x.VehiclePass.Contains(textToMatch)
                            || x.LockerNum.Contains(textToMatch)
                            || x.Visitor.Contact.Contains(textToMatch)
                            || x.VehicleNum.Contains(textToMatch)
                            || x.Visitor.RankAndName.Contains(textToMatch)
                            || (x.Escort?.RankAndName.Contains(textToMatch) ?? false));

                VisitorHistoryDataGrid.SelectedItems.Add(x);
                return true;
            };
        }

        private void VisitorHistoryDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        private void VisitorHistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            MyViewModel.SelectedVisitorRecords = VisitorHistoryDataGrid.SelectedItems.Cast<VisitorRecord>().ToList();
        }

        private void UpdateDatePicker(object sender, System.Windows.RoutedEventArgs e) {
            EndSearchDateEntry.DisplayDateEnd = DateTime.Today;

            if (StartSearchDateEntry.SelectedDate > EndSearchDateEntry.SelectedDate)
                StartSearchDateEntry.SelectedDate = EndSearchDateEntry.SelectedDate;

            StartSearchDateEntry.DisplayDateEnd = EndSearchDateEntry.SelectedDate;
        }
    }
}
