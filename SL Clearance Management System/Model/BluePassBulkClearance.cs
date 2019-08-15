using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SLCMS.Model {

    public class BluePassBulkClearance : INotifyPropertyChanged {
        private string _nric;
        private ClearanceLevelEnum _area;
        private DateTime _expiryDate;
        private string _clearanceDetails;
        private string _rank;
        private string _fullname;
        private string _unitorcompany;


        public string NRIC {
            get => _nric;
            set {
                _nric   = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(NRIC));
            }
        }
        public ClearanceLevelEnum AreaAccess {
            get => _area;
            set {
                _area = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(AreaAccess));
            }
        }
        public DateTime EndDate {
            get => _expiryDate;
            set {
                _expiryDate = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(EndDate));
            }
        }
        public string ClearanceDetails {
            get => _clearanceDetails;
            set {
                _clearanceDetails = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(ClearanceDetails));
            }
        }

        //If Personnel details are not present in data base
        public string Rank {
            get => _rank;
            set {
                _rank = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(Rank));
            }
        }
        public string FullName {
            get => _fullname;
            set {
                _fullname = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(FullName));
            }
        }
        public string Company {
            get => _unitorcompany;
            set {
                _unitorcompany = value;
                RaisePropertyChangedEvent(nameof(IsValid));
                RaisePropertyChangedEvent(nameof(Company));
            }
        }

        public bool IsValid => NRIC.Length == 9 && NRIC.All(x => char.IsLetterOrDigit(x) || x == '-')
                               && AreaAccess != ClearanceLevelEnum.None
                               && Rank.Trim().Length > 0
                               && Rank.Trim().Length < 6
                               && FullName.Trim().Length > 0
                               && Company.Trim().Length > 0;

        public BluePassBulkClearance Sterilise() {
            NRIC          = NRIC.Trim().ToUpper();
            ClearanceDetails = ClearanceDetails.Trim().ToUpper();
            Rank          = Rank.Trim().ToUpper();
            FullName      = FullName.Trim().ToUpper();
            Company       = Company.Trim().ToUpper();

            return this;
        }

        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}