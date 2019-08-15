using System.ComponentModel;
using SLCMS.BusinessLogic;

namespace SLCMS.Model
{
    public class VisitorPass : INotifyPropertyChanged {
        private string _passId;
        private ClearanceLevelEnum _areaAccess;
        private PassConditionEnum _passCondition;
        private int _orderOfPerference;
        private bool _requireEscort;
        private bool _isDirty;

        public string PassId {
            get => _passId;
            set {
                _passId = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(PassId));
            }
        }
        public ClearanceLevelEnum AreaAccess {
            get => _areaAccess;
            set {
                _areaAccess = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(AreaAccess));
            }
        }
        public PassConditionEnum PassCondition {
            get => _passCondition;
            set {
                _passCondition = value;
                IsDirty     = true;
                RaisePropertyChangedEvent(nameof(PassCondition));
            }
        }
        public bool RequireEscort {
            get => _requireEscort;
            set {
                _requireEscort = value;
                IsDirty        = true;
                RaisePropertyChangedEvent(nameof(PassCondition));
            }
        }
        public int OrderOfPerference {
            get => _orderOfPerference;
            set {
                _orderOfPerference = value;
                IsDirty        = true;
                RaisePropertyChangedEvent(nameof(OrderOfPerference));
            }
        }
        public bool IsDirty {
            get => _isDirty;
            set {
                _isDirty = value;
                RaisePropertyChangedEvent(nameof(IsDirty));
            }
        }
        public string AreaAccessString => Global.DataBase.FromClearanceLevelEnumToString(AreaAccess);
        public string PassConditionString => Global.DataBase.FromPassConditionEnumToString(PassCondition);


        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
