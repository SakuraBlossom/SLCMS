using System;

namespace SLCMS.Model
{
    public class VisitorRecord
    {
        public string NRIC => Visitor?.NRIC ?? "NA";
        public string EscortNRIC => Escort?.NRIC ?? "NA";


        public PersonnelDetails Visitor { get; set; }
        public PersonnelDetails Escort { get; set; }
        public VisitorPassEntry PassEntry { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string PersonnelPass { get; set; }
        public string VehiclePass { get; set; }
        public string LockerNum { get; set; }
        public string VehicleNum { get; set; }
        public bool InCampStatus { get; set; }
    }
}
