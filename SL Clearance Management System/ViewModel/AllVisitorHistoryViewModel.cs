using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.ViewModel {
    public class AllVisitorHistoryViewModel : INotifyPropertyChanged {
        public readonly TextBox SearchControlTextBox;

        #region [private Variables]

        //Visitor Selection Variables
        private bool _mutlipleEscortsPresentInSelectedVisitorRecords;
        private int _numberofVisitorRecordsSelected;
        private PersonnelDetails _visitorDetails;
        private PersonnelDetails _escortDetails;
        private IList<VisitorRecord> _selectedVisitorRecords;
        private IList<VisitorRecord> _visitorRecords;
        private DateTime _startDateSearch;
        private DateTime _endDateSearch;

        private ICommand _refreshVisitorDataCommand;
        private ICommand _bookOutVisitorDataCommand;

        #endregion

        #region [Properties]
        
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
                switch (NumberofVisitorRecordsSelected)
                {
                    case 0:
                        VisitorDetails = null;
                        EscortDetails  = null;

                        _mutlipleEscortsPresentInSelectedVisitorRecords = false;
                        break;

                    case 1:
                        VisitorDetails = _selectedVisitorRecords?[0]?.Visitor;
                        EscortDetails  = _selectedVisitorRecords?[0]?.Escort;

                        _mutlipleEscortsPresentInSelectedVisitorRecords = false;
                        break;

                    default:
                        if (NumberofVisitorRecordsSelected == 1) {
                            VisitorDetails = _selectedVisitorRecords?[0]?.Visitor;
                            EscortDetails  = _selectedVisitorRecords?[0]?.Escort;

                            _mutlipleEscortsPresentInSelectedVisitorRecords = false;
                        } else
                        {
                            VisitorDetails = null;

                            var listofEscorts = _selectedVisitorRecords?.Select(x => x?.Escort?.NRIC).Where(x => x != "NA").Distinct();
                            _mutlipleEscortsPresentInSelectedVisitorRecords = (listofEscorts != null && listofEscorts.Count() > 1);
                            EscortDetails = _mutlipleEscortsPresentInSelectedVisitorRecords
                                ? null
                                : _selectedVisitorRecords?[0]?.Escort;
                        }

                        break;
                }
            }
        }
        public IList<VisitorRecord> VisitorRecords
        {
            get => _visitorRecords;
            set {
                _visitorRecords = value;
                RaisePropertyChangedEvent(nameof(VisitorRecords));
            }
        }
        public DateTime StartDateSearch
        {
            get => _startDateSearch;
            set {
                _startDateSearch = value;
                RaisePropertyChangedEvent(nameof(StartDateSearch));
            }
        }
        public DateTime EndDateSearch
        {
            get => _endDateSearch;
            set {
                _endDateSearch = value;
                RaisePropertyChangedEvent(nameof(EndDateSearch));
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
        public ICommand BookInVisitorDataCommand {
            get => _bookOutVisitorDataCommand;
            set {
                _bookOutVisitorDataCommand = value;
                RaisePropertyChangedEvent(nameof(BookInVisitorDataCommand));
            }
        }

        #endregion


        public AllVisitorHistoryViewModel(TextBox searchControlTextBox, DatePicker startDatePicker, DatePicker endDatePicker) {
            SearchControlTextBox = searchControlTextBox;
            StartDateSearch = DateTime.Today;
            EndDateSearch = DateTime.Today;

            NumberofVisitorRecordsSelected = 0;
            VisitorRecords = new ObservableCollection<VisitorRecord>();

            RefreshVisitorDataCommand = new RelayCommand(async delegate {
                Global.MainViewModel.IsLoading = true;
                VisitorRecords = await Task.Run(() => Global.DataBase.GetBookOutHistoryRecords(StartDateSearch, EndDateSearch));
                searchControlTextBox.Clear();
                searchControlTextBox.Focus();
                Global.MainViewModel.IsLoading = false;
            });
            BookInVisitorDataCommand = new RelayCommand(delegate {
                if (SelectedVisitorRecords == null || SelectedVisitorRecords.Count == 0)
                    return;

                if(_mutlipleEscortsPresentInSelectedVisitorRecords)
                    switch (MessageBox.Show(
                        "The selected visitor entries do not share a common escort. Do you wish to proceed without parsing the escort's details?\n\nClick 'Yes' to porceed. Click 'No' to abort this operation.",
                        "No common escort selected",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            break;
                            
                        default:
                            return;
                    }

                _ = Global.MainViewModel.ViewModelBookInPersonnel.AddVisitorsToBookInList(
                    SelectedVisitorRecords,
                    EscortDetails);
            });
        }


        #region [Functions]
        

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
