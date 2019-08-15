using System.ComponentModel;
using System.Linq;
using SLCMS.BusinessLogic;

namespace SLCMS.Model
{
    public class VisitorPassEntry : INotifyPropertyChanged {
        private PersonnelDetails _visitorDetails;
        private string _passId;
        private string _vehiclePass;
        private string _lockerkey;
        private string _validationerror;
        private bool _isDirty;

        public VisitorPassEntry(PersonnelDetails personnelDetails) {
            _visitorDetails = personnelDetails;


            if(Global.MainViewModel.AutomaticallyRecommendVisitorPass)
                RunValidationTest(true);
            else
            {
                PersonnelPass = "";
                ValidationErrorString = "Automatic Pass selection was disabled";
            }
            VehiclePass = "";
            LockerNum = "";
            IsDirty = false;

        }

        public VisitorPassEntry(PersonnelDetails personnelDetails, string personnelPass, string vehiclePass, string lockerNum) {
            _visitorDetails = personnelDetails;
            _passId = personnelPass;
            _vehiclePass = vehiclePass;
            _lockerkey = lockerNum;
            IsDirty = false;
            RunValidationTest(false);
        }

        public bool HasNoValidationError => string.IsNullOrEmpty(_validationerror);
        public bool IsDirty {
            get => _isDirty;
            set {
                _isDirty = value;
                RaisePropertyChangedEvent(nameof(IsDirty));
            }
        }
        public string PersonnelPass {
            get => _passId;
            set {
                _passId = value.Trim();
                IsDirty = true;
                RunValidationTest(false);
                RaisePropertyChangedEvent(nameof(PersonnelPass));
            }
        }
        public string VehiclePass {
            get => _vehiclePass;
            set {
                _vehiclePass = value.Trim();
                IsDirty = true;

                if(string.IsNullOrWhiteSpace(PersonnelPass))
                    RunValidationTest(false);

                RaisePropertyChangedEvent(nameof(VehiclePass));
            }
        }
        public string LockerNum {
            get => _lockerkey;
            set {
                _lockerkey = value.Trim();
                IsDirty = true;

                if(string.IsNullOrWhiteSpace(PersonnelPass))
                    RunValidationTest(false);

                RaisePropertyChangedEvent(nameof(LockerNum));
            }
        }
        public string ValidationErrorString {
            get => _validationerror;
            set {
                _validationerror = value;
                RaisePropertyChangedEvent(nameof(ValidationErrorString));
                RaisePropertyChangedEvent(nameof(HasNoValidationError));
                Global.MainViewModel.ViewModelBookInPersonnel.ValidateAllBookInEntries(false, false);
            }
        }

        public void RunValidationTest(bool autoRecommendPass) {
            var byPassValidation = false;
            var errorString = string.Empty;
            VisitorPass possiblepass = null;

            if (_visitorDetails.IsBanStatus)
            {
                ValidationErrorString = "Personnel is Banned!";
                return;
            }

            if (string.IsNullOrWhiteSpace(PersonnelPass))
            {
                if (_visitorDetails.IsBasePersonnel && !(string.IsNullOrWhiteSpace(VehiclePass) && string.IsNullOrWhiteSpace(LockerNum)))
                    byPassValidation = true;
                else if(!Global.MainViewModel.AutomaticallyRecommendVisitorPass)
                    errorString = "No Visitor Pass issued.\r\nAutomatic Pass selection disabled.";
                else if (_visitorDetails.GetCurrentMaxClearanceLevel == null)
                    errorString = "Visitor has no valid clearance.";
                else if(_visitorDetails.IsBasePersonnel)
                    errorString = "No Visitor or Vehicle Pass issued.";
                else
                    errorString = "No Visitor Pass issued.";
            }
            else if (PersonnelPass.Length < 3)
                errorString = "Unrecognised Visitor Pass";
            else if (!Global.MainViewModel.ViewModelPassManagement.IsValidBluePass(PersonnelPass)) {
                possiblepass = Global.MainViewModel.ViewModelPassManagement.ListofVisitorPasses.FirstOrDefault(x => x.PassId == PersonnelPass);
                if(possiblepass == null)
                    errorString = "Unrecognised Visitor Pass";
                else if(!_visitorDetails.GetCurrentMaxClearanceLevel?.IsClearanceLevelEqualorAbove(possiblepass.AreaAccess) ?? true)
                    errorString = "Clearance requirement not met";
                else if(!_visitorDetails.IsBasePersonnel && !possiblepass.RequireEscort)
                    errorString = "Requires escort pass";
                else if(possiblepass.PassCondition != PassConditionEnum.Available)
                    errorString = "Pass was marked as Damaged/Lost";
                else if(!_visitorDetails.IsBasePersonnel && !possiblepass.RequireEscort)
                    errorString = "Issue a Civilian Pass";
                else if(_visitorDetails.IsBasePersonnel && possiblepass.RequireEscort)
                    errorString = "Issue a Military Pass";
            }

            if (string.IsNullOrEmpty(errorString) && !byPassValidation) {
                if (Global.MainViewModel.ViewModelDashBoard.HasPassBeenIssued(PersonnelPass))
                    errorString = "Visitor Pass was already issued";
                else if (Global.MainViewModel.ViewModelBookInPersonnel.ListofVisitorsBookingIn.Any(
                    x => x.PassEntry != this && x.PassEntry.PersonnelPass == PersonnelPass))
                    errorString = "Visitor Pass is pending distribution";
            }


            if (autoRecommendPass && !byPassValidation && !string.IsNullOrEmpty(errorString) && Global.MainViewModel.AutomaticallyRecommendVisitorPass) {
                _passId          = Global.MainViewModel.ViewModelPassManagement.RecommendVisitorPass(_visitorDetails);

                if (string.IsNullOrEmpty(_passId))
                   errorString = _visitorDetails.GetCurrentMaxClearanceLevel == null ? "Visitor has no valid clearance." : "No Visitor Pass issued.";
                else
                    errorString = string.Empty;

                RaisePropertyChangedEvent(nameof(PersonnelPass));

                possiblepass = Global.MainViewModel.ViewModelPassManagement.ListofVisitorPasses.FirstOrDefault(
                        x => x.PassId == _passId);
            }

            //Miscellous Validations
            if(Global.MainViewModel.ViewModelBookInPersonnel.EscortDetails?.NRIC == _visitorDetails.NRIC)
                errorString = "Visitor cannot escort oneself";
            else if (string.IsNullOrEmpty(errorString))
            {
                if (string.IsNullOrWhiteSpace(_visitorDetails.FullName))
                    errorString = "Name of Visitor must be filled";
                else if (string.IsNullOrWhiteSpace(_visitorDetails.UnitOrCompany))
                    errorString = "Visitor's Unit/Company must be filled";
                else if (string.IsNullOrWhiteSpace(_visitorDetails.Contact))
                    errorString = "Contact of Visitor must be filled";

                if (!_visitorDetails.IsBasePersonnel)
                {
                    if (Global.MainViewModel.ViewModelBookInPersonnel.EscortDetails == null)
                        errorString = "Personnel must be escorted";
                    else if (possiblepass != null
                             && !(Global.MainViewModel.ViewModelBookInPersonnel.EscortDetails
                                      ?.GetCurrentMaxClearanceLevel
                                      ?.IsClearanceLevelEqualorAbove(possiblepass.AreaAccess)
                                  ?? true))
                        errorString = "Escort Clearance requirement not met";
                }
            }

            ValidationErrorString = errorString;
        }

        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
