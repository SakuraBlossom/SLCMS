using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.ViewModel {
    public class VisitorHistoryLookupViewModel : INotifyPropertyChanged {
        #region [private Variables]

        //Visitor Selection Variables
        private bool _controlvisibility;
        private string _personnelNric;
        private string _nameofPersonnel;
        private VisitorRecord _currentSelectedRecord;
        private readonly Button _closeDialogueButton;

        private IList<VisitorRecord> _listofVisitorRecords;

        private ICommand _lookupVisitorCommand;
        private ICommand _lookupEscortCommand;
        private ICommand _closeControlCommand;

        #endregion

        #region [Visitor Selection Properties]

        public Visibility ControlVisibility {
            get => _controlvisibility ? Visibility.Visible : Visibility.Collapsed;
            set {
                _controlvisibility = (value == Visibility.Visible);
                RaisePropertyChangedEvent(nameof(ControlVisibility));
            }
        }
        public string PersonnelNric {
            get => _personnelNric;
            set {
                _personnelNric = value;
                RaisePropertyChangedEvent(nameof(PersonnelNric));
            }
        }
        public string PersonnelNric4D
        {
            get => _personnelNric?.Substring(5, 4) ?? "NA";
        }
        public string NameofPersonnel {
            get => _nameofPersonnel;
            set {
                _nameofPersonnel = value;
                RaisePropertyChangedEvent(nameof(NameofPersonnel));
            }
        }
        public VisitorRecord CurrentSelectedRecord {
            get => _currentSelectedRecord;
            set {
                _currentSelectedRecord = value;
                RaisePropertyChangedEvent(nameof(CurrentSelectedRecord));
                RaisePropertyChangedEvent(nameof(VisitorDetails));
                RaisePropertyChangedEvent(nameof(EscortDetails));
            }
        }
        public PersonnelDetails VisitorDetails => _currentSelectedRecord?.Visitor;
        public PersonnelDetails EscortDetails => _currentSelectedRecord?.Escort;
        public IList<VisitorRecord> ListofVisitorRecords {
            get => _listofVisitorRecords;
            set {
                _listofVisitorRecords = value;
                RaisePropertyChangedEvent(nameof(ListofVisitorRecords));
                RaisePropertyChangedEvent(nameof(NoRecordsFound));
            }
        }

        public bool NoRecordsFound => (_listofVisitorRecords?.Count == 0);

        public ICommand LookupVisitorCommand {
            get => _lookupVisitorCommand;
            set {
                _lookupVisitorCommand = value;
                RaisePropertyChangedEvent(nameof(LookupVisitorCommand));
            }
        }
        public ICommand LookupEscortCommand {
            get => _lookupEscortCommand;
            set {
                _lookupEscortCommand = value;
                RaisePropertyChangedEvent(nameof(_lookupEscortCommand));
            }
        }
        public ICommand CloseControlCommand {
            get => _closeControlCommand;
            set {
                _closeControlCommand = value;
                RaisePropertyChangedEvent(nameof(CloseControlCommand));
            }
        }
        #endregion

        public VisitorHistoryLookupViewModel(Button closeHistoryDialogueButton) {
            _closeDialogueButton = closeHistoryDialogueButton;
            ControlVisibility = Visibility.Collapsed;
            LookupEscortCommand = new RelayCommand(
                delegate {
                    var tolookup = _currentSelectedRecord?.Visitor;
                    if(tolookup != null)
                        LookupVisitorHistory(tolookup.NRIC, tolookup.RankAndName);
                });
            LookupEscortCommand = new RelayCommand(
                delegate {
                    var tolookup = _currentSelectedRecord?.Escort;
                    if(tolookup != null)
                        LookupVisitorHistory(tolookup.NRIC, tolookup.RankAndName);
                });
            CloseControlCommand = new RelayCommand(
                delegate {
                    ControlVisibility    = Visibility.Collapsed;
                    ListofVisitorRecords = null;
                });
        }

        public void LookupVisitorHistory(string visitornric, string rankandFullName) {

            if(ListofVisitorRecords != null && PersonnelNric == visitornric)
                return;

            PersonnelNric = visitornric;
            NameofPersonnel = visitornric.Substring(5,4) + " " + rankandFullName;
            ControlVisibility = Visibility.Visible;
            LookupVisitorHistoryhelper(visitornric);
            _closeDialogueButton.Focus();
        }
        public void LookupVisitorPassHistory(string visitorpassId) {
            PersonnelNric     = visitorpassId;
            NameofPersonnel   = visitorpassId;
            ControlVisibility = Visibility.Visible;
            LookupVisitorPassHistoryhelper(visitorpassId);
            _closeDialogueButton.Focus();
        }

        private async void LookupVisitorHistoryhelper(string visitornric) {
            Global.MainViewModel.IsLoading = true;
            ListofVisitorRecords = await Task.Run(() => Global.DataBase.GetVisitorHistory(visitornric));
            Global.MainViewModel.IsLoading = false;
        }
        private async void LookupVisitorPassHistoryhelper(string visitorpassId) {
            Global.MainViewModel.IsLoading = true;
            ListofVisitorRecords           = await Task.Run(() => Global.DataBase.GetVisitorPassHistory(visitorpassId));
            Global.MainViewModel.IsLoading = false;
        }

        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
