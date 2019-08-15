using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.ViewModel
{
    public class DashBoardViewModel : INotifyPropertyChanged {
        public readonly TextBox SearchControlTextBox;

        #region [private Variables]

        //Military Pass Count
        private int _numberofLoanedMilitaryPassAreaA;
        private int _numberofLoanedMilitaryPassAreaB;
        private int _numberofLoanedMilitaryPassAreaC;
        private int _numberofLoanedMilitaryPassAreaD;

        private int _totalNumberofMilitaryPassAreaA;
        private int _totalNumberofMilitaryPassAreaB;
        private int _totalNumberofMilitaryPassAreaC;
        private int _totalNumberofMilitaryPassAreaD;

        //Civilian Pass Count
        private int _numberofLoanedCivilianPassAreaA;
        private int _numberofLoanedCivilianPassAreaB;
        private int _numberofLoanedCivilianPassAreaC;
        private int _numberofLoanedCivilianPassAreaD;
        private int _numberofLoanedForeignPass;

        private int _totalNumberofCivilianPassAreaA;
        private int _totalNumberofCivilianPassAreaB;
        private int _totalNumberofCivilianPassAreaC;
        private int _totalNumberofCivilianPassAreaD;

        //Visitor Selection Variables
        private int _numberofVisitorRecordsSelected;
        private PersonnelDetails _visitorDetails;
        private PersonnelDetails _escortDetails;
        private IList<VisitorRecord> _selectedVisitorRecords;
        private IList<VisitorRecord> _visitorRecords;

        private ICommand _refreshVisitorDataCommand;
        private ICommand _bookOutVisitorDataCommand;

        #endregion

        #region [Properties]

        #region [Military Pass Count]

        public int TotalNumberofLoanedMilitaryPasses => _numberofLoanedMilitaryPassAreaA + _numberofLoanedMilitaryPassAreaB + _numberofLoanedMilitaryPassAreaC + _numberofLoanedMilitaryPassAreaD;
        public int TotalNumberofMilitaryPasses => _totalNumberofMilitaryPassAreaA + _totalNumberofMilitaryPassAreaB + _totalNumberofMilitaryPassAreaC + _totalNumberofMilitaryPassAreaD;

        public int NumberofLoanedMilitaryPassAreaA
        {
            get => _numberofLoanedMilitaryPassAreaA;
            set
            {
                _numberofLoanedMilitaryPassAreaA = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedMilitaryPassAreaA));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedMilitaryPasses));
            }
        }
        public int NumberofLoanedMilitaryPassAreaB
        {
            get => _numberofLoanedMilitaryPassAreaB;
            set
            {
                _numberofLoanedMilitaryPassAreaB = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedMilitaryPassAreaB));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedMilitaryPasses));
            }
        }
        public int NumberofLoanedMilitaryPassAreaC
        {
            get => _numberofLoanedMilitaryPassAreaC;
            set
            {
                _numberofLoanedMilitaryPassAreaC = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedMilitaryPassAreaC));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedMilitaryPasses));
            }
        }
        public int NumberofLoanedMilitaryPassAreaD
        {
            get => _numberofLoanedMilitaryPassAreaD;
            set
            {
                _numberofLoanedMilitaryPassAreaD = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedMilitaryPassAreaD));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedMilitaryPasses));
            }
        }


        public int TotalNumberofMilitaryPassAreaA
        {
            get => _totalNumberofMilitaryPassAreaA;
            set
            {
                _totalNumberofMilitaryPassAreaA = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPassAreaA));
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPasses));
            }
        }
        public int TotalNumberofMilitaryPassAreaB
        {
            get => _totalNumberofMilitaryPassAreaB;
            set
            {
                _totalNumberofMilitaryPassAreaB = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPassAreaB));
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPasses));
            }
        }
        public int TotalNumberofMilitaryPassAreaC
        {
            get => _totalNumberofMilitaryPassAreaC;
            set
            {
                _totalNumberofMilitaryPassAreaC = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPassAreaC));
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPasses));
            }
        }
        public int TotalNumberofMilitaryPassAreaD
        {
            get => _totalNumberofMilitaryPassAreaD;
            set
            {
                _totalNumberofMilitaryPassAreaD = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPassAreaD));
                RaisePropertyChangedEvent(nameof(TotalNumberofMilitaryPasses));
            }
        }

        #endregion

        #region [Civilian Pass Count]

        public int TotalNumberofLoanedCivilianPasses => _numberofLoanedCivilianPassAreaA + _numberofLoanedCivilianPassAreaB + _numberofLoanedCivilianPassAreaC + _numberofLoanedCivilianPassAreaD + TotalNumberofForeignPass;
        public int TotalNumberofCivilianPasses => _totalNumberofCivilianPassAreaA + _totalNumberofCivilianPassAreaB + _totalNumberofCivilianPassAreaC + _totalNumberofCivilianPassAreaD + TotalNumberofForeignPass;

        public int NumberofLoanedCivilianPassAreaA
        {
            get => _numberofLoanedCivilianPassAreaA;
            set
            {
                _numberofLoanedCivilianPassAreaA = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedCivilianPassAreaA));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedCivilianPasses));
            }
        }
        public int NumberofLoanedCivilianPassAreaB
        {
            get => _numberofLoanedCivilianPassAreaB;
            set
            {
                _numberofLoanedCivilianPassAreaB = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedCivilianPassAreaB));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedCivilianPasses));
            }
        }
        public int NumberofLoanedCivilianPassAreaC
        {
            get => _numberofLoanedCivilianPassAreaC;
            set
            {
                _numberofLoanedCivilianPassAreaC = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedCivilianPassAreaC));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedCivilianPasses));
            }
        }
        public int NumberofLoanedCivilianPassAreaD
        {
            get => _numberofLoanedCivilianPassAreaD;
            set
            {
                _numberofLoanedCivilianPassAreaD = value;
                RaisePropertyChangedEvent(nameof(NumberofLoanedCivilianPassAreaD));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedCivilianPasses));
            }
        }
        public int TotalNumberofForeignPass {
            get => _numberofLoanedForeignPass;
            set {
                _numberofLoanedForeignPass = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofForeignPass));
                RaisePropertyChangedEvent(nameof(TotalNumberofLoanedCivilianPasses));
            }
        }


        public int TotalNumberofCivilianPassAreaA
        {
            get => _totalNumberofCivilianPassAreaA;
            set
            {
                _totalNumberofCivilianPassAreaA = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPassAreaA));
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPasses));
            }
        }
        public int TotalNumberofCivilianPassAreaB
        {
            get => _totalNumberofCivilianPassAreaB;
            set
            {
                _totalNumberofCivilianPassAreaB = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPassAreaB));
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPasses));
            }
        }
        public int TotalNumberofCivilianPassAreaC
        {
            get => _totalNumberofCivilianPassAreaC;
            set
            {
                _totalNumberofCivilianPassAreaC = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPassAreaC));
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPasses));
            }
        }
        public int TotalNumberofCivilianPassAreaD
        {
            get => _totalNumberofCivilianPassAreaD;
            set
            {
                _totalNumberofCivilianPassAreaD = value;
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPassAreaD));
                RaisePropertyChangedEvent(nameof(TotalNumberofCivilianPasses));
            }
        }

        #endregion

        #region [Visitor Selection Properties]

        public int NumberofVisitorRecordsSelected
        {
            get => _numberofVisitorRecordsSelected;
            set
            {
                _numberofVisitorRecordsSelected = value;
                RaisePropertyChangedEvent(nameof(NumberofVisitorRecordsSelected));
            }
        }
        public PersonnelDetails VisitorDetails {
            get => _visitorDetails;
            set {
                _visitorDetails = value;
                RaisePropertyChangedEvent(nameof(VisitorDetails));
            }
        }
        public PersonnelDetails EscortDetails
        {
            get => _escortDetails;
            set
            {
                _escortDetails = value;
                RaisePropertyChangedEvent(nameof(EscortDetails));
            }
        }
        public IList<VisitorRecord> SelectedVisitorRecords
        {
            get => _selectedVisitorRecords;
            set
            {
                _selectedVisitorRecords = value;
                RaisePropertyChangedEvent(nameof(SelectedVisitorRecords));
                
                NumberofVisitorRecordsSelected = _selectedVisitorRecords?.Count ?? 0;
                if (NumberofVisitorRecordsSelected == 1) {
                    VisitorDetails = _selectedVisitorRecords?[0]?.Visitor;
                    EscortDetails = _selectedVisitorRecords?[0]?.Escort;
                } else
                {
                    VisitorDetails = null;
                    EscortDetails = _selectedVisitorRecords?.Select(x => x?.Escort?.NRIC).Where(x => x != "NA").Distinct().Count() == 1
                        ? _selectedVisitorRecords[0]?.Escort
                        : null;
                }


            }
        }
        public IList<VisitorRecord> VisitorRecords
        {
            get => _visitorRecords;
            set {
                _visitorRecords = value;
                RecaculatePasses();
                RaisePropertyChangedEvent(nameof(VisitorRecords));
            }
        }
        #endregion

        #endregion

        #region [Relay Commands]
        public ICommand RefreshVisitorDataCommand
        {
            get => _refreshVisitorDataCommand;
            set {
                _refreshVisitorDataCommand = value;
                RaisePropertyChangedEvent(nameof(RefreshVisitorDataCommand));
            }
        }
        public ICommand BookOutVisitorDataCommand {
            get => _bookOutVisitorDataCommand;
            set {
                _bookOutVisitorDataCommand = value;
                RaisePropertyChangedEvent(nameof(BookOutVisitorDataCommand));
            }
        }

        #endregion


        public DashBoardViewModel(TextBox searchControlTextBox) {
            SearchControlTextBox = searchControlTextBox;

            NumberofVisitorRecordsSelected = 0;
            VisitorRecords = new ObservableCollection<VisitorRecord>();

            RefreshVisitorDataCommand = new RelayCommand(async delegate {
                Global.MainViewModel.IsLoading = true;
                VisitorRecords = await Task.Run(() => Global.DataBase.GetVisitorRecords());
                searchControlTextBox.Clear();
                searchControlTextBox.Focus();
                Global.MainViewModel.IsLoading = false;
            });
            BookOutVisitorDataCommand = new RelayCommand(async delegate {
                if (SelectedVisitorRecords == null || SelectedVisitorRecords.Count == 0)
                    return;

                Global.MainViewModel.IsLoading = true;
                VisitorRecords = await Task.Run(() => Global.DataBase.BookOutVisitorRecords(SelectedVisitorRecords));
                searchControlTextBox.Clear();
                searchControlTextBox.Focus();
                Global.MainViewModel.IsLoading = false;
            });
        }

        #region [Functions]

        public bool HasPassBeenIssued(string passId) {
            return VisitorRecords.Any(x => x.PersonnelPass == passId);
        }

        public void RecaculatePasses() {
            if(Global.MainViewModel?.ViewModelPassManagement?.ListofVisitorPasses == null)
                return;

            var listofAllVisitorPasses = Global.MainViewModel.ViewModelPassManagement.ListofVisitorPasses;
            TotalNumberofMilitaryPassAreaA = listofAllVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.A && !x.RequireEscort);
            TotalNumberofMilitaryPassAreaB = listofAllVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.AB && !x.RequireEscort);
            TotalNumberofMilitaryPassAreaC = listofAllVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.ABC && !x.RequireEscort);
            TotalNumberofMilitaryPassAreaD = listofAllVisitorPasses.Count(x => (x.AreaAccess == ClearanceLevelEnum.ABCD || x.AreaAccess == ClearanceLevelEnum.ABCDW) && !x.RequireEscort);

            TotalNumberofCivilianPassAreaA = listofAllVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.A && x.RequireEscort);
            TotalNumberofCivilianPassAreaB = listofAllVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.AB && x.RequireEscort);
            TotalNumberofCivilianPassAreaC = listofAllVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.ABC && x.RequireEscort);
            TotalNumberofCivilianPassAreaD = listofAllVisitorPasses.Count(x => (x.AreaAccess == ClearanceLevelEnum.ABCD || x.AreaAccess == ClearanceLevelEnum.ABCDW) && x.RequireEscort);


            var matchVisitorPasses = listofAllVisitorPasses.Where(
                x => !string.IsNullOrWhiteSpace(x.PassId)
                     && VisitorRecords.Any(record => record.PersonnelPass == x.PassId)
                     ).ToArray();

            TotalNumberofForeignPass = VisitorRecords.Count(x => !string.IsNullOrWhiteSpace(x.PersonnelPass) && x.PersonnelPass != "NA") - matchVisitorPasses.Count();
            if(TotalNumberofForeignPass <= 0)
                TotalNumberofForeignPass = 0;

            NumberofLoanedMilitaryPassAreaA = matchVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.A && !x.RequireEscort);
            NumberofLoanedMilitaryPassAreaB = matchVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.AB && !x.RequireEscort);
            NumberofLoanedMilitaryPassAreaC = matchVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.ABC && !x.RequireEscort);
            NumberofLoanedMilitaryPassAreaD = matchVisitorPasses.Count(x => (x.AreaAccess == ClearanceLevelEnum.ABCD || x.AreaAccess == ClearanceLevelEnum.ABCDW) && !x.RequireEscort);

            NumberofLoanedCivilianPassAreaA = matchVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.A && x.RequireEscort);
            NumberofLoanedCivilianPassAreaB = matchVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.AB && x.RequireEscort);
            NumberofLoanedCivilianPassAreaC = matchVisitorPasses.Count(x => x.AreaAccess == ClearanceLevelEnum.ABC && x.RequireEscort);
            NumberofLoanedCivilianPassAreaD = matchVisitorPasses.Count(x => (x.AreaAccess == ClearanceLevelEnum.ABCD || x.AreaAccess == ClearanceLevelEnum.ABCDW) && x.RequireEscort);
        }

        #endregion


        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
