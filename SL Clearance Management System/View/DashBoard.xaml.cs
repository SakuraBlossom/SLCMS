using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS.View
{
    /// <summary>
    /// Interaction logic for DashBoard.xaml
    /// </summary>
    public partial class DashBoard
    {
        public DashBoardViewModel MyViewModel;

        public DashBoard()
        {
            InitializeComponent();
            DataContext = MyViewModel = new DashBoardViewModel(SearchTextBox);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var textToMatch = SearchTextBox?.Text;
            var cv          = CollectionViewSource.GetDefaultView(VisitorInCampDataGrid.ItemsSource);

            if (string.IsNullOrEmpty(textToMatch)|| textToMatch == "NA" || !((VisitorInCampDataGrid.ItemsSource) is IList<VisitorRecord>))
            {
                cv.Filter = null;
                return;
            }

            VisitorInCampDataGrid.SelectedItems.Clear();
            cv.Filter = arg => {
                if(!(arg is VisitorRecord x))
                    return false;

                if (x.NRIC == textToMatch || x.EscortNRIC == textToMatch || x.PersonnelPass == textToMatch ||
                    x.VehiclePass == textToMatch || x.LockerNum == textToMatch) {
                    VisitorInCampDataGrid.SelectedItems.Add(x);
                    return true;
                }

                return (x.NRIC.Contains(textToMatch)
                        || x.EscortNRIC.Contains(textToMatch)
                        || x.PersonnelPass.Contains(textToMatch)
                        || x.VehiclePass.Contains(textToMatch)
                        || x.LockerNum.Contains(textToMatch)
                        || x.Visitor.Contact.Contains(textToMatch)
                        || x.VehicleNum.Contains(textToMatch)
                        || x.Visitor.RankAndName.Contains(textToMatch)
                        || (x.Escort?.RankAndName.Contains(textToMatch) ?? false));
            };
        }

        private void VisitorInCampDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        private void VisitorInCampDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            MyViewModel.SelectedVisitorRecords = VisitorInCampDataGrid.SelectedItems.Cast<VisitorRecord>().ToList();
        }
    }
}
