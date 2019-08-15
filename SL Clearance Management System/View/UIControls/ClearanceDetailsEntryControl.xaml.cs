using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.Model;

namespace SLCMS.View.UIControls {
    /// <summary>
    /// Interaction logic for ClearanceDetailsEntryControl.xaml
    /// </summary>
    public partial class ClearanceDetailsEntryControl {
        public DateTime LastUpdateTiming;

        public ClearanceDetailsEntryControl() {
            InitializeComponent();
            ResetView();
            AdHocAreaAccessEntry.ItemsSource = Enum.GetValues(typeof(ClearanceLevelEnum)).Cast<ClearanceLevelEnum>().Skip(1);
            LastUpdateTiming = DateTime.Today;

            this.DataContextChanged += delegate {

                if(LastUpdateTiming.Equals(DateTime.Today))
                    return;

                AdHocStartDateEntry.SelectedDate   = DateTime.Today;
                AdHocStartDateEntry.DisplayDateEnd = DateTime.Today;

                AdHocEndDateEntry.SelectedDate     = DateTime.Today;
                AdHocEndDateEntry.DisplayDateStart = DateTime.Today;

            };
        }
        private void LoadingRow(object sender, DataGridRowEventArgs e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();

        public void ResetView() {
            AdHocAreaAccessEntry.SelectedItem = ClearanceLevelEnum.A;
            AdHocStartDateEntry.SelectedDate = DateTime.Today;
            AdHocStartDateEntry.DisplayDateEnd = DateTime.Today;

            AdHocEndDateEntry.SelectedDate   = DateTime.Today;
            AdHocEndDateEntry.DisplayDateStart = DateTime.Today;

            AdHocDetailsEntry.Text = "AD-HOC";
        }

        private void AddAdHocClearance(object sender, RoutedEventArgs e) {

            if(!(DataContext is PersonnelDetails currentPersonnelDetail)
                || AdHocStartDateEntry.SelectedDate == null
                || AdHocEndDateEntry.SelectedDate == null
                || currentPersonnelDetail.ListofClearance.Any(clearance => (
                   clearance.AreaAccess == (ClearanceLevelEnum)AdHocAreaAccessEntry.SelectedValue
                   && clearance.StartDate == (DateTime)AdHocStartDateEntry.SelectedDate
                   && clearance.EndDate == (DateTime)AdHocEndDateEntry.SelectedDate
                   && clearance.ClearanceDetails == AdHocDetailsEntry.Text
               )))
                return;

            currentPersonnelDetail.AddClearance(new Clearance {
                AreaAccess       = (ClearanceLevelEnum)AdHocAreaAccessEntry.SelectedValue,
                StartDate        = (DateTime) AdHocStartDateEntry.SelectedDate,
                EndDate          = (DateTime) AdHocEndDateEntry.SelectedDate,
                ClearanceDetails = AdHocDetailsEntry.Text
            });
        }

        private void DeleteSelectedClearance(object sender, RoutedEventArgs e) {
            if(!(DataContext is PersonnelDetails currentPersonnelDetail)
               || !(VisitorClearanceDataGrid.SelectedItem is Clearance details))
                return;

            currentPersonnelDetail.RemoveClearance(details);
        }

        private void AdHocAreaAccessEntry_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch(e.Key) {
                case Key.A:
                    AdHocAreaAccessEntry.SelectedValue = ClearanceLevelEnum.A;
                    break;
                case Key.B:
                    AdHocAreaAccessEntry.SelectedValue = ClearanceLevelEnum.AB;
                    break;
                case Key.C:
                    AdHocAreaAccessEntry.SelectedValue = ClearanceLevelEnum.ABC;
                    break;
                case Key.D:
                    AdHocAreaAccessEntry.SelectedValue = ClearanceLevelEnum.ABCD;
                    break;
                case Key.W:
                    AdHocAreaAccessEntry.SelectedValue = ClearanceLevelEnum.ABCDW;
                    break;
                case Key.Tab: return;
            }
            e.Handled = true;
        }
    }
}
