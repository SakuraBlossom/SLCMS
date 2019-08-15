using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS.View.UIControls {
    /// <summary>
    ///     Interaction logic for SelectEscortPersonnelDialogueControl.xaml
    /// </summary>
    public partial class SelectEscortPersonnelDialogueControl {
        private bool _isloadingVisitorDetails;
        public IList<PersonnelDetails> ListofEscortDetails;

        public SelectEscortPersonnelDialogueControl() {
            InitializeComponent();
            _isloadingVisitorDetails = false;

            IsVisibleChanged += SelectEscortPersonnel_IsVisibleChanged;
            SearchTextBox.PreviewKeyDown += delegate (object sender, KeyEventArgs e) {
                if (e.Key == Key.Enter) {
                    _ = SearchforEscortVisitorDetails();
                }
                if (!(Key.A <= e.Key && e.Key <= Key.Z || Key.D0 <= e.Key && e.Key <= Key.D9 ||
                      Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9 ||
                      e.Key == Key.Back || e.Key == Key.Delete ||
                      e.Key == Key.Left || e.Key == Key.Right ||
                      e.Key == Key.Space || e.Key == Key.Tab))
                    e.Handled = true;
            };
            EscortPersonnelDetailsEntryBox.ButtonCommand = new RelayCommand(delegate {
                ListofPossibleEscortDataGrid.SelectedIndex = -1;
                Visibility = Visibility.Collapsed;
            });
        }

        private void SelectEscortPersonnel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((e.NewValue is Visibility visibility ? visibility : Visibility.Hidden) == Visibility.Visible) {
                this.UpdateLayout();
                SearchTextBox.SelectAll();
                SearchTextBox.Focus();

                ListofPossibleEscortDataGrid.SelectedIndex = 0;
            }
        }

        private void VisitorInCampDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void UpdatePersonnelDetailsButton_Click(object sender, RoutedEventArgs e) {
            _ = UpdatePersonnelDetails();
        }

        private void CloseDialogButton_Click(object sender, RoutedEventArgs e) {
            if (DataContext == null || (DataContext is PersonnelDetails escort && escort.PersonnelIsValidEscort))
                Visibility = Visibility.Collapsed;
        }

        private void SearchPersonnelButton_Click(object sender, RoutedEventArgs e) {
            _ = SearchforEscortVisitorDetails();
        }

        private async Task SearchforEscortVisitorDetails() {
            var searchvalue = SearchTextBox.Text;
            if (_isloadingVisitorDetails || searchvalue.Length < 4)
                return;

            _isloadingVisitorDetails = true;
            Global.MainViewModel.IsLoading = true;
            var searchResult = await Task.Run(() => Global.DataBase.SearchEscortPersonnelDetails(searchvalue));

            ListofPossibleEscortDataGrid.ItemsSource = searchResult;
            ListofPossibleEscortDataGrid.SelectedIndex = 0;

            if (searchResult.Count == 0)
                SearchTextBox.Focus();
            else if (searchResult[0]?.GetCurrentMaxClearanceLevel == null)
                VisitorClearanceManagementControl.AdHocAreaAccessEntry.Focus();
            else
                ConfirmEscortButton.Focus();

            Global.MainViewModel.IsLoading = false;
            _isloadingVisitorDetails = false;
        }

        private async Task UpdatePersonnelDetails() {
            if (_isloadingVisitorDetails || !(DataContext is PersonnelDetails escort))
                return;

            _isloadingVisitorDetails = true;
            Global.MainViewModel.IsLoading = true;
            await Task.Run(() => Global.DataBase.UpdateVisitorPersonnelDetails(escort));
            Global.MainViewModel.IsLoading = false;
            _isloadingVisitorDetails = false;
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                _ = SearchforEscortVisitorDetails();
            }
        }
    }
}