using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.ViewModel {
    public class PassManagementViewModel : INotifyPropertyChanged {
        public static readonly Regex BluePassRegex = new Regex(@"\bPF[0-9]{3,}\b");
        private IList<VisitorPass> _listofVisitorPasses;
        private ICommand           _refreshVisitorPassCommand;
        private ICommand           _saveAllCommand;

        public PassManagementViewModel() {
            RefreshVisitorPassCommand = new RelayCommand(
                async delegate {
                    Global.MainViewModel.IsLoading = true;
                    ListofVisitorPasses            = await Task.Run(() => Global.DataBase.GetVisitorPasses());
                    Global.MainViewModel.IsLoading = false;
                });
            SaveAllCommand = new RelayCommand(
                async delegate {
                    Global.MainViewModel.IsLoading = true;
                    await Task.Run(() => Global.DataBase.UpdateVisitorPasses(ListofVisitorPasses));
                    Global.MainViewModel.ViewModelDashBoard.RefreshVisitorDataCommand.Execute(null);
                    Global.MainViewModel.IsLoading = false;
                });

            ListofVisitorPasses = Global.DataBase.GetVisitorPasses();
        }

        public IList<VisitorPass> ListofVisitorPasses {
            get => _listofVisitorPasses;
            set {
                _listofVisitorPasses = value;
                RaisePropertyChangedEvent(nameof(ListofVisitorPasses));
            }
        }

        public string RecommendVisitorPass(PersonnelDetails personnelDetails) {
            var match = BluePassRegex.Match(personnelDetails.Remarks);
            if(match.Success)
                return match.Value.Trim();

            var highestClearance = personnelDetails.GetCurrentMaxClearanceLevel?.AreaAccess ?? ClearanceLevelEnum.None;
            switch (highestClearance)
            {
                case ClearanceLevelEnum.None:
                    return string.Empty;
                case ClearanceLevelEnum.ABCDW:
                    highestClearance = ClearanceLevelEnum.ABCD;
                    break;
            }

            var passesAlreadyIssued = Global.MainViewModel.ViewModelDashBoard.VisitorRecords.Select(x => x.PersonnelPass);
            var passesToBeIssued = Global.MainViewModel.ViewModelBookInPersonnel.ListofVisitorsBookingIn.Select(x => x.PassEntry.PersonnelPass).Where(x => !string.IsNullOrEmpty(x));

            var possibleVisitorPass = ListofVisitorPasses.FirstOrDefault(
                x => x.PassCondition == PassConditionEnum.Available && x.AreaAccess == highestClearance &&
                     x.RequireEscort != personnelDetails.IsBasePersonnel && passesAlreadyIssued.All(z => z != x.PassId) &&
                     passesToBeIssued.All(y => y != x.PassId));

            return possibleVisitorPass?.PassId ?? string.Empty;
        }

        public bool IsValidBluePass(string passId) {
            return BluePassRegex.IsMatch(passId);
        }

        public void RemoveVisitorPass(VisitorPass model) {
            if (!ListofVisitorPasses.Remove(model))
                return;

            var currentPassIndex = model.OrderOfPerference;
            foreach (var pass in ListofVisitorPasses)
            {
                if (pass.OrderOfPerference < currentPassIndex)
                    continue;

                pass.OrderOfPerference -= 1;
            }
        }

        public void MoveUp(VisitorPass model) {
            if (ListofVisitorPasses.Count < 1 || model.OrderOfPerference <= 1 || !ListofVisitorPasses.Contains(model))
                return;

            var tempPass = model;
            var newIndex = model.OrderOfPerference - 1;
            ListofVisitorPasses[newIndex]     = ListofVisitorPasses[newIndex - 1];
            ListofVisitorPasses[newIndex - 1] = tempPass;

            ListofVisitorPasses[newIndex].OrderOfPerference     = newIndex + 1;
            ListofVisitorPasses[newIndex - 1].OrderOfPerference = newIndex;

            RaisePropertyChangedEvent(nameof(ListofVisitorPasses));
        }

        public void MoveDown(VisitorPass model) {
            if (ListofVisitorPasses.Count < 2 || model.OrderOfPerference >= ListofVisitorPasses.Count ||
                !ListofVisitorPasses.Contains(model))
                return;

            var tempPass = model;
            var newIndex = model.OrderOfPerference - 1;
            ListofVisitorPasses[newIndex]     = ListofVisitorPasses[newIndex + 1];
            ListofVisitorPasses[newIndex + 1] = tempPass;

            ListofVisitorPasses[newIndex].OrderOfPerference     = newIndex + 1;
            ListofVisitorPasses[newIndex + 1].OrderOfPerference = newIndex + 2;

            RaisePropertyChangedEvent(nameof(ListofVisitorPasses));
        }

        #region [Visitor Selection Commands]

        public ICommand RefreshVisitorPassCommand {
            get => _refreshVisitorPassCommand;
            set {
                _refreshVisitorPassCommand = value;
                RaisePropertyChangedEvent(nameof(RefreshVisitorPassCommand));
            }
        }

        public ICommand SaveAllCommand {
            get => _saveAllCommand;
            set {
                _saveAllCommand = value;
                RaisePropertyChangedEvent(nameof(SaveAllCommand));
            }
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