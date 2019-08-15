using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS.View
{
    /// <summary>
    /// Interaction logic for BookInPersonnel.xaml
    /// </summary>
    public partial class BookInPersonnel {
        public BookInPersonnelViewModel MyViewModel;

        public BookInPersonnel()
        {
            InitializeComponent();
            DataContext = MyViewModel = new BookInPersonnelViewModel(SearchTextBox, BookInButtonToolTip, PassNumberEntryBox, VisitorPersonnelParticularsEntryBox, VisitorClearanceControl);

            VisitorPersonnelParticularsEntryBox.ButtonCommand = new RelayCommand(
                delegate {
                    MyViewModel.RemoveVisitorEntry(VisitorInCampDataGrid.SelectedIndex);
                    VisitorInCampDataGrid.Items.Refresh();
                });

            EscortPersonnelEntryBox.ButtonCommand = new RelayCommand(
                delegate {
                    SelectEscortPersonnelDialog.Visibility = System.Windows.Visibility.Visible;
                });
        }

        private void VisitorInCampDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();

        private void VisitorInCampDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(!(VisitorInCampDataGrid.SelectedValue is VisitorRecord visitorDetails))
                return;

            MyViewModel.CurrentSelectedRecord = visitorDetails;
            if(visitorDetails.Visitor != null && string.IsNullOrWhiteSpace(visitorDetails.Visitor.FullName))
                VisitorPersonnelParticularsEntryBox.NameEntryBox.Focus();
            else {
                PassNumberEntryBox.SelectAll();
                PassNumberEntryBox.Focus();
            }
        }

        private void BookOutButton_PreviewKeyDown(object sender, KeyEventArgs e) {

            if (e.Key != Key.Tab)
                return;

            VisitorPersonnelParticularsEntryBox.NameEntryBox.SelectAll();
            VisitorPersonnelParticularsEntryBox.NameEntryBox.Focus();
            e.Handled = true;
        }
    }
}
