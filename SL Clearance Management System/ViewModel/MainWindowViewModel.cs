using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SLCMS.BusinessLogic;

namespace SLCMS.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged {
        private bool _autoRecommendVisitorPass;
        private bool _isCivilianCounter;
        private bool _isloading;
        private int _selectedTabIndex;

        public Action TabControlChangedEvent;

        public DashBoardViewModel ViewModelDashBoard;
        public BookInPersonnelViewModel ViewModelBookInPersonnel;
        public AllVisitorHistoryViewModel ViewModelAllVisitorHistory;
        public VisitorHistoryLookupViewModel ViewModelVisitorHistoryLookup;
        public PassManagementViewModel ViewModelPassManagement;

        private ICommand _compactDataBaseCommand;
        private ICommand _resetEntireDataBaseButtonCommand;

        #region [Properties]

        public bool AutomaticallyRecommendVisitorPass {
            get => _autoRecommendVisitorPass;
            set {
                _autoRecommendVisitorPass = value;
                RaisePropertyChangedEvent(nameof(AutomaticallyRecommendVisitorPass));
            }
        }
        public bool IsCivilianCounter
        {
            get => _isCivilianCounter;
            set {
                _isCivilianCounter = value;
                RaisePropertyChangedEvent(nameof(IsCivilianCounter));
            }
        }
        public bool IsLoading {
            get => _isloading;
            set {
                _isloading = value;
                RaisePropertyChangedEvent(nameof(IsLoading));
            }
        }
        public int SelectedTabIndex {
            get => _selectedTabIndex;
            set {
                _selectedTabIndex = value;
                RaisePropertyChangedEvent(nameof(SelectedTabIndex));
                TabControlChangedEvent?.Invoke();
            }
        }

        #endregion

        #region [ICommands]
        public ICommand CompactDataBaseCommand {
            get => _compactDataBaseCommand;
            set {
                _compactDataBaseCommand = value;
                RaisePropertyChangedEvent(nameof(CompactDataBaseCommand));
            }
        }
        public ICommand ResetEntireDataBaseButtonCommand {
            get => _resetEntireDataBaseButtonCommand;
            set {
                _resetEntireDataBaseButtonCommand = value;
                RaisePropertyChangedEvent(nameof(ResetEntireDataBaseButtonCommand));
            }
        }

        #endregion

        public MainWindowViewModel() {
            CompactDataBaseCommand = new RelayCommand(async delegate {
                Global.MainViewModel.IsLoading = true;
                await Task.Run(() => Global.DataBase.CompactDataBase(true));
                Global.MainViewModel.IsLoading = false;
            });
            ResetEntireDataBaseButtonCommand = new RelayCommand(async delegate {
                Global.MainViewModel.IsLoading = true;
                await Task.Run(() => Global.DataBase.OverrideDataBaseWithDefault(false));
                Global.MainViewModel.ViewModelDashBoard.RefreshVisitorDataCommand.Execute(null);
                Global.MainViewModel.ViewModelBookInPersonnel.ResetAllEntryCommand.Execute(null);
                Global.MainViewModel.IsLoading = false;
            });

            AutomaticallyRecommendVisitorPass = true;
            IsCivilianCounter = true;
            SelectedTabIndex = 0;
        }

        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
