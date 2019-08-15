using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;
using SLCMS.View.UIControls;

namespace SLCMS.ViewModel {
    public class BookInPersonnelViewModel : INotifyPropertyChanged {
        public readonly ToolTip                          BookInStatusToolTip;
        public readonly TextBox                          SearchControlTextBox;
        public readonly TextBox                          VisitorPassDetailsControl;
        public readonly PersonnelParticularsEntryControl VisitorPersonnelParticularsEntryControl;
        public readonly ClearanceDetailsEntryControl VisitorClearanceEntryControl;

        #region [private Variables]

        //Visitor Selection Variables
        private bool _isloadingVisitorDetails;
        private string _searchByNricStr;
        private VisitorRecord _currentRecord;
        private IList<VisitorRecord> _listofVisitorsBookingIn;
        private PersonnelDetails _escortPersonnel;
        private ICommand _addVisitorCommand;
        private ICommand _resetAllEntryCommand;
        private ICommand _updatePersonnelDetailsCommand;
        private ICommand _bookInAllPersonnelCommand;

        #endregion

        #region [Visitor Selection Properties]

        public int NumberofVisitorBookingInRecords => _listofVisitorsBookingIn.Count;
        public bool IsValidBookInEntry => (ListofVisitorsBookingIn.Count > 0 &&
                                           ListofVisitorsBookingIn.All(x => !x.Visitor.IsBanStatus && x.PassEntry.HasNoValidationError));

        public string SearchByNricStr {
            get => _searchByNricStr;
            set {
                _searchByNricStr = value;
                RaisePropertyChangedEvent(nameof(SearchByNricStr));
            }
        }

        public PersonnelDetails VisitorDetails => _currentRecord?.Visitor;
        public VisitorPassEntry VisitorEntryDetails => _currentRecord?.PassEntry;

        public PersonnelDetails EscortDetails {
            get => _escortPersonnel;
            set {
                _escortPersonnel = value;
                ValidateAllBookInEntries(true, false);
                RaisePropertyChangedEvent(nameof(EscortDetails));
            }
        }

        public VisitorRecord CurrentSelectedRecord {
            get => _currentRecord;
            set {
                _currentRecord = value;
                RaisePropertyChangedEvent(nameof(CurrentSelectedRecord));
                RaisePropertyChangedEvent(nameof(VisitorEntryDetails));
                RaisePropertyChangedEvent(nameof(VisitorDetails));
            }
        }

        public IList<VisitorRecord> ListofVisitorsBookingIn {
            get => _listofVisitorsBookingIn;
            set {
                _listofVisitorsBookingIn = value;
                RaisePropertyChangedEvent(nameof(CurrentSelectedRecord));
                RaisePropertyChangedEvent(nameof(EscortDetails));
                RaisePropertyChangedEvent(nameof(VisitorDetails));
                RaisePropertyChangedEvent(nameof(VisitorEntryDetails));
                RaisePropertyChangedEvent(nameof(ListofVisitorsBookingIn));
                RaisePropertyChangedEvent(nameof(NumberofVisitorBookingInRecords));
            }
        }


        #endregion

        #region [Visitor Selection Commands]

        public ICommand ResetAllEntryCommand {
            get => _resetAllEntryCommand;
            set {
                _resetAllEntryCommand = value;
                RaisePropertyChangedEvent(nameof(ResetAllEntryCommand));
            }
        }

        public ICommand UpdatePersonnelDetailsCommand {
            get => _updatePersonnelDetailsCommand;
            set {
                _updatePersonnelDetailsCommand = value;
                RaisePropertyChangedEvent(nameof(UpdatePersonnelDetailsCommand));
            }
        }

        public ICommand BookInAllPersonnelCommand {
            get => _bookInAllPersonnelCommand;
            set {
                _bookInAllPersonnelCommand = value;
                RaisePropertyChangedEvent(nameof(BookInAllPersonnelCommand));
            }
        }


        public ICommand AddVisitorCommand {
            get => _addVisitorCommand;
            set {
                _addVisitorCommand = value;
                RaisePropertyChangedEvent(nameof(AddVisitorCommand));
            }
        }

        #endregion


        public BookInPersonnelViewModel(
                TextBox                          searchControlTextBox,
                ToolTip                          bookInStatusToolTip,
                TextBox                          visitorPassDetailsControl,
                PersonnelParticularsEntryControl personnelParticularsEntryControl,
                ClearanceDetailsEntryControl     clearanceControl) {
            SearchControlTextBox                    = searchControlTextBox;
            BookInStatusToolTip                     = bookInStatusToolTip;
            VisitorPassDetailsControl               = visitorPassDetailsControl;
            VisitorPersonnelParticularsEntryControl = personnelParticularsEntryControl;
            VisitorClearanceEntryControl            = clearanceControl;
            _isloadingVisitorDetails                = false;
            ListofVisitorsBookingIn                 = new ObservableCollection<VisitorRecord>();


            bookInStatusToolTip.Opened += async delegate {
                await Task.Delay(3000);
                bookInStatusToolTip.IsOpen = false;
            };

            AddVisitorCommand = new RelayCommand(
                delegate {
                    _ = AddVisitorToBookInList();
                });
            BookInAllPersonnelCommand = new RelayCommand(
                async delegate {
                    ValidateAllBookInEntries(true, false);
                    if(!IsValidBookInEntry)
                        return;

                    Global.MainViewModel.IsLoading = true;
                    var successful = await Task.Run(
                        () => Global.DataBase.BookInAllPersonnel(ListofVisitorsBookingIn, EscortDetails));

                    if (successful) {
                        ResetAllEntryCommand.Execute(null);
                        Global.MainViewModel.ViewModelDashBoard.RefreshVisitorDataCommand.Execute(null);
                        Global.MainViewModel.SelectedTabIndex = 0;
                    }
                    Global.MainViewModel.IsLoading = false;

                });
            UpdatePersonnelDetailsCommand = new RelayCommand(
                async delegate {
                    if (_isloadingVisitorDetails)
                        return;

                    _isloadingVisitorDetails       = true;
                    Global.MainViewModel.IsLoading = true;
                    await Task.Run(() => Global.DataBase.UpdateVisitorPersonnelDetails(VisitorDetails));
                    Global.MainViewModel.IsLoading = false;
                    _isloadingVisitorDetails       = false;
                });
            ResetAllEntryCommand = new RelayCommand(
                delegate {
                    ListofVisitorsBookingIn.Clear();
                    EscortDetails         = null;
                    CurrentSelectedRecord = null;
                    VisitorClearanceEntryControl.ResetView();

                    RaisePropertyChangedEvent(nameof(ListofVisitorsBookingIn));
                    RaisePropertyChangedEvent(nameof(NumberofVisitorBookingInRecords));

                    Global.ForceGarbageCollector();
                });
            
            SearchControlTextBox.PreviewKeyDown += delegate(object sender, KeyEventArgs e) {
                BookInStatusToolTip.IsOpen = false;
                if (e.Key == Key.Enter && SearchControlTextBox.Text.Length >= 5)
                    AddVisitorCommand.Execute(null);
                if (!((Key.A <= e.Key && e.Key <= Key.Z) || (Key.D0 <= e.Key && e.Key <= Key.D9)
                      || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) || e.Key == Key.Back || e.Key == Key.Delete
                      || e.Key == Key.Left || e.Key == Key.Right
                      || e.Key == Key.Tab|| e.Key == Key.Subtract || e.Key == Key.OemMinus))
                    e.Handled = true;
            };
        }


        #region [Functions]

        //Check Escort
        //Check Personnel already booked In
        //Ask to recommend Pass

        public async Task AddVisitorsToBookInList(IList<VisitorRecord> toInsertVisitor, PersonnelDetails escort) {
            Global.MainViewModel.IsLoading = true;
            if (ListofVisitorsBookingIn.Count > 0 || EscortDetails != null)
            {
                switch (MessageBox.Show(
                    "Do you wish to overwrite your pre-existing list of personnel pending to book-in?\n\nClick 'Yes' to overwrite.\nClick 'No' to combine both list of entries.\nClick 'Cancel' to abort this operation.",
                    "Overwrite existing list of personnel pending Book-In",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        ResetAllEntryCommand.Execute(null);
                        break;

                    case MessageBoxResult.No:
                        if (!(EscortDetails != null && escort != null && EscortDetails.NRIC == escort.NRIC))
                            MessageBox.Show(
                                "The following visitor entries have conflicting escorts. The current escort should be resolved by user.",
                                "Conflicting escort details",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                        break;

                    default:
                        Global.MainViewModel.IsLoading = false;
                        return;
                }
            }
            

            var enableAutoRecommendPass = false;
            var personnelHasAlreadybookedInOrPending = 0;

            switch (MessageBox.Show(
                "Would you like this software to recommend new Visitor Passes to be issued?\n\nClick 'Yes' to proceed.\nClick 'No' to indicate the previous passes issued.\nClick 'Cancel' to abort this operation.",
                "Automatically recommend a Visitor Pass",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    enableAutoRecommendPass = true;
                    break;

                case MessageBoxResult.No:
                    break;

                default:
                    Global.MainViewModel.IsLoading = false;
                    return;
            }

            EscortDetails = null;
            foreach (var visitorpersonnel in toInsertVisitor)
            {
                //Check whether personnel has already booked in
                if(Global.MainViewModel.ViewModelDashBoard.VisitorRecords.Any(x => x.NRIC == visitorpersonnel.Visitor.NRIC)) {
                    personnelHasAlreadybookedInOrPending++;
                    continue;
                }

                //Check if personnel is already in current book in record
                if(ListofVisitorsBookingIn.Any(x => x.NRIC == visitorpersonnel.Visitor.NRIC)) {
                    personnelHasAlreadybookedInOrPending++;
                    continue;
                }

                var visitordetails = await Task.Run(() => Global.DataBase.GetVisitorPersonnelDetails(visitorpersonnel.NRIC));
                var newRecord = new VisitorRecord {
                    Visitor   = visitordetails,
                    PassEntry = (enableAutoRecommendPass)
                        ? new VisitorPassEntry(visitordetails)
                        : new VisitorPassEntry(visitordetails, visitorpersonnel.PersonnelPass, visitorpersonnel.VehiclePass, visitorpersonnel.LockerNum)
                };
                
                ListofVisitorsBookingIn.Add(newRecord);
                CurrentSelectedRecord = newRecord;
            }

            //Load Details of Escort
            EscortDetails = (escort != null)
                ? Global.DataBase.GetVisitorPersonnelDetails(escort.NRIC)
                : null;

            if (personnelHasAlreadybookedInOrPending > 0)
            {
                if(personnelHasAlreadybookedInOrPending == 1)
                    MessageBox.Show(
                        "1 entry was ignored due to duplication of request or the visitor had already Booked-In",
                        "Entries were either duplicated or visitor had already Booked-In",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                else
                    MessageBox.Show(
                        $"{personnelHasAlreadybookedInOrPending} entries were ignored due to duplication of request or the respective visitors had already Booked-In",
                        "Entries were either duplicated or visitors had already Booked-In",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }

            Global.MainViewModel.IsLoading = false;

            Global.MainViewModel.SelectedTabIndex = 1;
            SearchControlTextBox.Focus();
        }

        public async Task AddVisitorToBookInList() {
            SearchByNricStr = SearchControlTextBox.Text.Trim().ToUpper();
            if(_isloadingVisitorDetails || SearchByNricStr.Length < 5)
                return;

            //Check whether personnel is in list of visitors waiting to book in
            var recordSelection = ListofVisitorsBookingIn.FirstOrDefault(
                x => x.PassEntry?.PersonnelPass == SearchByNricStr || x.PassEntry?.VehiclePass == SearchByNricStr);
            if(recordSelection != null) {
                CurrentSelectedRecord = recordSelection;
                VisitorPassDetailsControl.SelectAll();
                VisitorPassDetailsControl.Focus();
                return;
            }

            if (SearchByNricStr.Length < 8)
                return;

            //User is searching for Pass
            if(SearchByNricStr.Length < 9)
                SearchByNricStr = SearchByNricStr.PadRight(9, '-');
            else if(SearchByNricStr.Length > 9)
                SearchByNricStr = SearchByNricStr.Substring(0, 9);

            //Check whether personnel has already booked in
            if(Global.MainViewModel.ViewModelDashBoard.VisitorRecords.Any(x => x.NRIC == SearchByNricStr)) {
                BookInStatusToolTip.Content = SearchByNricStr + " has already booked in";
                BookInStatusToolTip.IsOpen = true;
                return;
            }

            //Check if personnel is already in current book in record
            _isloadingVisitorDetails = true;
            _currentRecord = null;
            CurrentSelectedRecord = ListofVisitorsBookingIn.FirstOrDefault(x => x.NRIC == SearchByNricStr);

            if (CurrentSelectedRecord == null)
            {
                Global.MainViewModel.IsLoading = true;
                var visitordetails = await Task.Run(() => Global.DataBase.GetVisitorPersonnelDetails(SearchByNricStr));
                var newRecord = new VisitorRecord {
                    Visitor   = visitordetails,
                    PassEntry = new VisitorPassEntry(visitordetails)
                };

                ListofVisitorsBookingIn.Add(newRecord);
                CurrentSelectedRecord          = newRecord;
                Global.MainViewModel.IsLoading = false;

                if (string.IsNullOrWhiteSpace(newRecord.Visitor.FullName))
                {
                    VisitorPersonnelParticularsEntryControl.NameEntryBox.Focus();
                }
                else if (newRecord.Visitor.GetCurrentMaxClearanceLevel == null) {
                    VisitorClearanceEntryControl.AdHocAreaAccessEntry.Focus();
                } 
                else {
                    VisitorPassDetailsControl.SelectAll();
                    VisitorPassDetailsControl.Focus();
                }
            }

            SearchByNricStr = string.Empty;
            RaisePropertyChangedEvent(nameof(NumberofVisitorBookingInRecords));

            ValidateAllBookInEntries(true, false);
            _isloadingVisitorDetails = false;
        }

        public void RemoveVisitorEntry(int index) {
            if(0 <= index && index < NumberofVisitorBookingInRecords)
                _listofVisitorsBookingIn.RemoveAt(index);

            RaisePropertyChangedEvent(nameof(EscortDetails));
            RaisePropertyChangedEvent(nameof(VisitorDetails));
            RaisePropertyChangedEvent(nameof(CurrentSelectedRecord));
            RaisePropertyChangedEvent(nameof(ListofVisitorsBookingIn));
            RaisePropertyChangedEvent(nameof(NumberofVisitorBookingInRecords));

            ValidateAllBookInEntries(false, false);
        }

        public void ValidateAllBookInEntries(bool forceReValidate, bool recommendPass) {

            if (forceReValidate) {
                foreach(var currententry in ListofVisitorsBookingIn) { currententry.PassEntry?.RunValidationTest(recommendPass && (currententry == CurrentSelectedRecord)); }
            }
            RaisePropertyChangedEvent(nameof(IsValidBookInEntry));
        }

        #endregion

        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChangedEvent(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}