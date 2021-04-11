using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SLCMS.BusinessLogic;

namespace SLCMS.Model
{
    public class PersonnelDetails : INotifyPropertyChanged {
        private string _nric;
        private string _rank;
        private string _fullname;
        private string _contact;
        private string _unitorcompany;
        private string _vehicleNum;
        private string _remarks;

        private bool _isBanStatus;
        private bool _isBasePersonnel;
        private bool _allowedToEscort;
        private bool _isDirty;

        private IList<Clearance> _listofClearance;

        public bool IsDirty
        {
            get => _isDirty;
            set {
                _isDirty = value;
                Global.MainViewModel.ViewModelBookInPersonnel.ValidateAllBookInEntries(true, true);
                RaisePropertyChangedEvent(nameof(IsDirty));
                RaisePropertyChangedEvent(nameof(PersonnelIsValidEscort));
            }
        }


        public string NRIC
        {
            get => _nric;
            set
            {
                _nric = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(NRIC));
            }
        }
        public string NRIC4D
        {
            get => _nric.Substring(5, 4);
        }
        public string Rank
        {
            get => _rank;
            set
            {
                _rank = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(Rank));
                RaisePropertyChangedEvent(nameof(RankAndName));
            }
        }
        public string FullName
        {
            get => _fullname;
            set
            {
                _fullname = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(FullName));
                RaisePropertyChangedEvent(nameof(RankAndName));
            }
        }
        public string Contact
        {
            get => _contact;
            set
            {
                _contact = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(Contact));
            }
        }
        public string UnitOrCompany
        {
            get => _unitorcompany;
            set
            {
                _unitorcompany = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(UnitOrCompany));
            }
        }
        public string VehicleNum
        {
            get => _vehicleNum;
            set
            {
                _vehicleNum = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(VehicleNum));
            }
        }
        public string Remarks
        {
            get => _remarks;
            set
            {
                _remarks = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(Remarks));
            }
        }


        public bool IsBasePersonnel
        {
            get => _isBasePersonnel;
            set
            {
                _isBasePersonnel = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(IsBasePersonnel));
            }
        }
        public bool AllowedToEscort
        {
            get => _allowedToEscort;
            set
            {
                _allowedToEscort = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(AllowedToEscort));
            }
        }
        public bool IsBanStatus
        {
            get => _isBanStatus;
            set
            {
                _isBanStatus = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(IsBanStatus));
            }
        }
        public IList<Clearance> ListofClearance
        {
            get => _listofClearance;
            set
            {
                _listofClearance = value;
                IsDirty = true;
                RaisePropertyChangedEvent(nameof(ListofClearance));
                RaisePropertyChangedEvent(nameof(GetCurrentMaxClearanceLevel));
            }
        }


        public string RankAndName => (Rank + ' ' + FullName);
        public bool PersonnelIsValidEscort => 
            !string.IsNullOrWhiteSpace(FullName)
            && !string.IsNullOrWhiteSpace(Contact)
            && !string.IsNullOrWhiteSpace(UnitOrCompany)
            && (GetCurrentMaxClearanceLevel?.HasAreaAccess ?? false)
            && !IsBanStatus
            && AllowedToEscort;


        public void RemoveClearance(Clearance selectedClearance)
        {
            if (selectedClearance == null)
                return;

            _listofClearance.Remove(selectedClearance);
            IsDirty = true;
            RaisePropertyChangedEvent(nameof(ListofClearance));
            RaisePropertyChangedEvent(nameof(GetCurrentMaxClearanceLevel));
        }
        public void AddClearance(Clearance selectedClearance)
        {
            if(_listofClearance == null)
                _listofClearance = new List<Clearance>();

            selectedClearance.NRIC = NRIC;
            _listofClearance.Add(selectedClearance);
            IsDirty = true;
            RaisePropertyChangedEvent(nameof(ListofClearance));
            RaisePropertyChangedEvent(nameof(GetCurrentMaxClearanceLevel));
        }
        

        public void Sterilise() {
            NRIC = NRIC.Trim().ToUpper();
            Rank = Rank.Trim().ToUpper();
            FullName = FullName.Trim().ToUpper();
            Contact = Contact.Trim().ToUpper();
            UnitOrCompany = UnitOrCompany.Trim().ToUpper();
            VehicleNum = VehicleNum.Trim().ToUpper();
            Remarks = Remarks.Trim().ToUpper();
        }

        public Clearance GetCurrentMaxClearanceLevel => ListofClearance?
            .Where(x => x.IsClearanceValid)
            .OrderByDescending(x => (byte)x.AreaAccess)
            .FirstOrDefault();

        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
