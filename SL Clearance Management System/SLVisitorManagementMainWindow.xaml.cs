using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SLCMS.BusinessLogic;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS
{
    /// <summary>
    /// Interaction logic for SLVisitorManagementMainWindow.xaml
    /// </summary>
    public partial class SLVisitorManagementMainWindow {

        public SLVisitorManagementMainWindow() {
            Global.DataBase = new SLDataBaseConnection();

            InitializeComponent();
            Global.SLVisitorManagementMainWindow = this;
            DataContext = Global.MainViewModel = new MainWindowViewModel {
                ViewModelDashBoard = ViewControlDashboard.MyViewModel,
                ViewModelBookInPersonnel = ViewControlPersonnelPassBookIn.MyViewModel,
                ViewModelAllVisitorHistory = ViewControlHistory.MyViewModel,
                ViewModelVisitorHistoryLookup = VisitorHistoryLookupControl.MyViewModel,
                ViewModelPassManagement = ViewControlPassManagement.MyViewModel,
                TabControlChangedEvent = TabChangedEvent,
                IsLoading = false
            };
            
            Global.MainViewModel.ViewModelDashBoard.RefreshVisitorDataCommand.Execute(null);
            EventManager.RegisterClassHandler(typeof(Window), Keyboard.PreviewKeyDownEvent, new KeyEventHandler(KeyDown), true);
            ViewControlDashboard.SearchTextBox.Focus();

        }

        public void TabChangedEvent() {
            if(VisitorHistoryLookupControl.Visibility == Visibility.Visible || LoadingScreen.Visibility == Visibility.Visible)
                return;

            UpdateLayout();
            ViewMainTabControl.UpdateLayout();
            switch(Global.MainViewModel.SelectedTabIndex) {
                case 0:
                    ViewControlDashboard.SearchTextBox.Clear();
                    ViewControlDashboard.SearchTextBox.Focus();
                    break;
                case 1:
                    if(ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.Visibility == Visibility.Visible) {
                        ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.SearchTextBox.Clear();
                        ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.SearchTextBox.Focus();
                    } else {
                        ViewControlPersonnelPassBookIn.SearchTextBox.Clear();
                        ViewControlPersonnelPassBookIn.SearchTextBox.Focus();
                    }
                    break;
                case 2:
                    ViewControlHistory.SearchTextBox.Clear();
                    ViewControlHistory.SearchTextBox.Focus();

                    ViewControlHistory.StartSearchDateEntry.DisplayDateEnd = DateTime.Today;
                    ViewControlHistory.EndSearchDateEntry.DisplayDateEnd   = DateTime.Today;

                    Global.MainViewModel.ViewModelAllVisitorHistory.StartDateSearch = DateTime.Today.AddDays(-1);
                    Global.MainViewModel.ViewModelAllVisitorHistory.EndDateSearch = DateTime.Today;

                    Global.MainViewModel.ViewModelAllVisitorHistory.RefreshVisitorDataCommand.Execute(null);
                    break;
                case 3:
                    ViewControlPassManagement.SearchTextBox.Clear();
                    ViewControlPassManagement.SearchTextBox.Focus();
                    break;
            }
        }

        private new void KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key)
            {
                case Key.PageUp:
                    Global.MainViewModel.SelectedTabIndex = 0;
                    break;
                case Key.PageDown:
                    Global.MainViewModel.SelectedTabIndex = 1;
                    Global.MainViewModel.ViewModelBookInPersonnel.ValidateAllBookInEntries(true, true);
                    break;
                case Key.Home:
                    TabChangedEvent();
                    break;

                case Key.Up:
                    if((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && ViewMainTabControl.SelectedIndex == 1)
                    {
                        var currentSelectedItem = Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.VisitorInCampDataGrid.SelectedIndex;
                        var itemCount           = Global.MainViewModel.ViewModelBookInPersonnel.ListofVisitorsBookingIn.Count;

                        if(itemCount > 1)
                            Global.MainViewModel.ViewModelBookInPersonnel.CurrentSelectedRecord =
                                Global.MainViewModel.ViewModelBookInPersonnel.ListofVisitorsBookingIn[
                                    (currentSelectedItem > 0) ? (currentSelectedItem - 1) : (itemCount - 1)];
                    }
                    break;

                case Key.Down:
                    if((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && ViewMainTabControl.SelectedIndex == 1)
                    {
                        var currentSelectedItem = Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.VisitorInCampDataGrid.SelectedIndex;
                        var itemCount           = Global.MainViewModel.ViewModelBookInPersonnel.ListofVisitorsBookingIn.Count;

                        if(itemCount > 1)
                            Global.MainViewModel.ViewModelBookInPersonnel.CurrentSelectedRecord =
                                Global.MainViewModel.ViewModelBookInPersonnel.ListofVisitorsBookingIn[
                                    (currentSelectedItem + 1 < itemCount) ? (currentSelectedItem + 1) : 0];
                    }
                    break;

                //For BookIn Screen
                case Key.Insert:
                    if(ViewMainTabControl.SelectedIndex != 1)
                        return;

                    switch(Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.Visibility) {
                        case Visibility.Visible:
                            if (!(Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.DataContext is PersonnelDetails personnel) || !personnel.PersonnelIsValidEscort)
                            {
                                Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog
                                    .ListofPossibleEscortDataGrid.SelectedIndex = -1;
                            }

                            Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.Visibility = Visibility.Collapsed;
                            break;
                            
                        default:

                            Global.SLVisitorManagementMainWindow.ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.Visibility
                                = Visibility.Visible;

                            ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.SearchTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                                ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.SearchTextBox.SelectAll();
                                ViewControlPersonnelPassBookIn.SelectEscortPersonnelDialog.SearchTextBox.Focus();
                            }));
                            break;
                        }
                    break;

                case Key.End:
                    switch (ViewMainTabControl.SelectedIndex)
                    {
                        case 0: Global.MainViewModel.ViewModelDashBoard.BookOutVisitorDataCommand?.Execute(null);
                            break;
                        case 1: Global.MainViewModel.ViewModelBookInPersonnel.BookInAllPersonnelCommand?.Execute(null);
                            break;
                        default:
                            return;
                    }
                    break;

                case Key.Tab:
                case Key.Space:
                    return;

                default:
                    if(ViewMainTabControl.SelectedIndex != 0)
                        return;

                    if(ViewControlDashboard.SearchTextBox.IsFocused)
                        return;

                    ViewControlDashboard.SearchTextBox.Focus();
                    ViewControlDashboard.SearchTextBox.SelectAll();

                    return;
            }
            ViewMainTabControl.UpdateLayout();
            e.Handled = true;
        }
    }
}
