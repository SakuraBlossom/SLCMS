using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLCMS.Model;

namespace SLCMS.BusinessLogic
{
    public class FakeDataTest
    {
        private static readonly Random random = new Random();
        protected static string GenerateOtp(int length, bool allowAlphabets = true)
        {
            var characters = allowAlphabets
                ? "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                : "1234567890";
            return new string(Enumerable.Repeat(characters, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            /*
            var otp = string.Empty;
            for (var i = 0; i < length; i++)
            {
                string character;
                do
                {
                    var index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character, StringComparison.Ordinal) != -1);
                otp += character;
            }
            return otp;
             */
        }
        
        public static ObservableCollection<PersonnelDetails> GenerateSampleArrayPersonnelDetails() {

            return new ObservableCollection<PersonnelDetails> {
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails(),
                GenerateSamplePersonnelDetails()
            };
        }

        public static ObservableCollection<Clearance> GenerateSampleClearanceArray() {
            return new ObservableCollection<Clearance> {
                new Clearance { NRIC="???", AreaAccess = ClearanceLevelEnum.ABC, ClearanceDetails = "LEVEL3", StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2) },
                new Clearance { NRIC="???", AreaAccess = ClearanceLevelEnum.AB, ClearanceDetails = "LEVEL2", StartDate = DateTime.Today.AddDays(-3), EndDate = DateTime.Today.AddDays(-2) },
                new Clearance { NRIC="???", AreaAccess = ClearanceLevelEnum.ABCDW, ClearanceDetails = "LEVEL5", StartDate = DateTime.Today.AddDays(-3), EndDate = DateTime.Today.AddDays(2) },
                new Clearance { NRIC="???", AreaAccess = ClearanceLevelEnum.ABCD, ClearanceDetails = "LEVEL4", StartDate = DateTime.Today.AddDays(-2), EndDate = DateTime.Today.AddDays(5) }
            };
        }

        public static PersonnelDetails GenerateSamplePersonnelDetails(string nric = null) {
            return new PersonnelDetails { NRIC = nric ?? GenerateOtp(9), Rank = "MR", FullName = GenerateOtp(80), Contact = GenerateOtp(8, false), UnitOrCompany = GenerateOtp(20), VehicleNum = GenerateOtp(8), IsBasePersonnel = true, AllowedToEscort = false, ListofClearance = GenerateSampleClearanceArray(), IsBanStatus = false, Remarks = "", IsDirty = false };
        }

    }
}
