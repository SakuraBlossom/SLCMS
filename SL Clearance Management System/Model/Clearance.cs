using System;
using SLCMS.BusinessLogic;

namespace SLCMS.Model {
    public class Clearance {
        public string             NRIC             { get; set; }
        public ClearanceLevelEnum AreaAccess       { get; set; }
        public DateTime           StartDate        { get; set; }
        public DateTime           EndDate          { get; set; }
        public string             ClearanceDetails { get; set; }

        public bool   HasAreaAccess    => AreaAccess != ClearanceLevelEnum.None;
        public string AreaAccessString => Global.DataBase.FromClearanceLevelEnumToString(AreaAccess);

        public bool IsClearanceValid =>
            StartDate.Date.CompareTo(DateTime.Today) <= 0 && EndDate.Date.CompareTo(DateTime.Today) >= 0;

        public bool IsClearanceLevelEqualorAbove(ClearanceLevelEnum minimumclearanceleve) {
            return (byte) AreaAccess >= (byte) minimumclearanceleve;
        }
    }
}