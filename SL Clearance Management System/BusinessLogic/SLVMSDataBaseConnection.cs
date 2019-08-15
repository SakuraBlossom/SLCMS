using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using SLCMS.Model;

namespace SLCMS.BusinessLogic {
    public class SLDataBaseConnection {
        public readonly string OleDbProvider;
        public readonly string DataBaseLocation;
        private readonly string _connectionString;
        private readonly string _locationOfMicrosoftAccess;

        #region [Initialisation]

        public SLDataBaseConnection(string dataBaseLocation = null) {
            DataBaseLocation = dataBaseLocation ??
                               (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\EIPVS.accdb");
            //DataBaseLocation = dataBaseLocation ?? (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\SLCMS DB.accdb") ;

            //Identify SQL Provider
            var listofOleDbProviders = new[] {
                "Microsoft.ACE.OLEDB.21.0", "Microsoft.ACE.OLEDB.20.0","Microsoft.ACE.OLEDB.19.0",
                "Microsoft.ACE.OLEDB.18.0", "Microsoft.ACE.OLEDB.17.0", "Microsoft.ACE.OLEDB.16.0",
                "Microsoft.ACE.OLEDB.15.0", "Microsoft.ACE.OLEDB.14.0", "Microsoft.ACE.OLEDB.13.0",
                "Microsoft.ACE.OLEDB.12.0", "Microsoft.Jet.OLEDB.8.0", "Microsoft.Jet.OLEDB.7.0",
                "Microsoft.Jet.OLEDB.6.0", "Microsoft.Jet.OLEDB.5.0", "Microsoft.Jet.OLEDB.4.0",
                "Microsoft.Jet.OLEDB.3.5"
            };
            var listofAvailableOleDbProviders = new OleDbEnumerator().GetElements();

            OleDbProvider = listofOleDbProviders.FirstOrDefault(
                provider => (listofAvailableOleDbProviders.Select($"SOURCES_NAME = \'{provider}'").Length > 0));

            if(OleDbProvider == null) {
                MessageBox.Show(
                    
                    "No supported OleDb provider is registered on the local machine.\r\n\r\n" +
                    "This software relies on the support of Microsoft Access (2010 to 2016 edition) to operate.",
                    "Missing dependency",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Environment.Exit(1);
            }

            var listofMicrosoftAccessExecutableLocation = new[] {
                @"C:\Program Files\Microsoft Office\Office\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office10\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office11\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office12\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office14\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office15\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office15\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office16\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office16\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office17\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office17\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office18\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office18\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office19\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office19\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office20\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office20\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office21\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office21\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office22\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office22\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office23\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office23\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office24\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office24\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\Office25\MSACCESS.exe",
                @"C:\Program Files\Microsoft Office\root\Office25\MSACCESS.exe"
            };
            _locationOfMicrosoftAccess = listofMicrosoftAccessExecutableLocation.FirstOrDefault(File.Exists);
            if(string.IsNullOrEmpty(_locationOfMicrosoftAccess)) {
                MessageBox.Show(
                    
                    "Microsoft Access executable not found.\r\n\r\n" +
                    "This software relies on the support of Microsoft Access to operate.",
                    "Missing dependency",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Environment.Exit(1);
            }

            _connectionString = $"Provider=\'{OleDbProvider}\';Data Source=\'{DataBaseLocation}\'";
            if(RunConnectionTest())
                return;

            var msgResult = MessageBox.Show(
                
                "Visitor Management DataBase file is either missing or corrupt.\r\n" +
                "If this error contiunes to persist, resetting the Data Base may resolve this error. " +
                "However, all records will be lost!\r\n\r\n" + "Do you wish to proceed with resetting the Data Base?",
                "Missing or corrupt Visitor Management DataBase",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error,
                MessageBoxResult.No);

            if(msgResult == MessageBoxResult.Yes)
                OverrideDataBaseWithDefault(true);
            else
                Environment.Exit(0);
        }

        [HandleProcessCorruptedStateExceptions]
        public bool RunConnectionTest() {

            var actionSuccessful = false;
            var vmsDatabase = new OleDbConnection(_connectionString);

            try {
                vmsDatabase.Open();
                OleDbTransaction trans = vmsDatabase.BeginTransaction();

                var comm = new OleDbCommand {
                    CommandText =
                        "UPDATE PersonnelDetails SET RANK=\'REC\', FULLNAME=\'NA\', CONTACT=\'\', COMPANY=\'NA\', VEHICLENUM=\'\', IsMilitaryPersonnel=0, AllowToEscort=0, BAN=0, REMARKS=\'\', LASTEDIT=@_currentyear WHERE NRIC=\'NA\'",
                    Connection = vmsDatabase,
                    Transaction = trans
                };
                comm.Parameters.AddWithValue("@_currentyear", DateTime.Now.Year.ToString());

                if(comm.ExecuteNonQuery() == 0) {
                    comm.Parameters.Clear();
                    comm.CommandText =
                        "INSERT INTO PersonnelDetails(NRIC, RANK, FULLNAME, CONTACT, COMPANY, VEHICLENUM, IsMilitaryPersonnel, AllowToEscort, BAN, REMARKS, LASTEDIT) VALUES (\'NA\', \'REC\', \'NA\', \'\', \'NA\', \'NA\', FALSE, FALSE, FALSE, \'\', @_currentyear)";
                    comm.Parameters.AddWithValue("@_currentyear", DateTime.Now.Year.ToString());
                    comm.ExecuteNonQuery();
                }

                trans.Commit();

                comm.Dispose();
                trans.Dispose();
                actionSuccessful = true;
            } catch(Exception ex) {
                MessageBox.Show(
                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return actionSuccessful;
        }

        ~SLDataBaseConnection() { }

        #endregion

        #region [Parse Enum]

        public PassConditionEnum FromStringToPassConditionEnum(string str) {
            switch(str.ToUpper()) {
                case "AVAILABLE":
                    return PassConditionEnum.Available;
                case "LOST":
                    return PassConditionEnum.Lost;
                case "DAMAGED":
                    return PassConditionEnum.Damaged;
                default:
                    return PassConditionEnum.Available;
            }
        }

        public string FromPassConditionEnumToString(PassConditionEnum myenum) {
            switch(myenum) {
                case PassConditionEnum.Lost:
                    return "LOST";
                case PassConditionEnum.Damaged:
                    return "DAMAGED";
                default:
                    return "AVAILABLE";
            }
        }

        public ClearanceLevelEnum FromStringToClearanceLevelEnum(string str) {
            switch(str) {
                case "A":
                    return ClearanceLevelEnum.A;
                case "B":
                case "AB":
                    return ClearanceLevelEnum.AB;
                case "C":
                case "ABC":
                    return ClearanceLevelEnum.ABC;
                case "D":
                case "ABCD":
                    return ClearanceLevelEnum.ABCD;
                case "W":
                case "ABCDW":
                    return ClearanceLevelEnum.ABCDW;
                default:
                    return ClearanceLevelEnum.None;
            }
        }

        public string FromClearanceLevelEnumToString(ClearanceLevelEnum myenum) {
            switch(myenum) {
                case ClearanceLevelEnum.A:
                    return "A";
                case ClearanceLevelEnum.AB:
                    return "AB";
                case ClearanceLevelEnum.ABC:
                    return "ABC";
                case ClearanceLevelEnum.ABCD:
                    return "ABCD";
                case ClearanceLevelEnum.ABCDW:
                    return "ABCDW";

                default:
                    return "None";
            }
        }

        #endregion

        #region [Reader Parser Functions]

        public PersonnelDetails ReadToPersonnelDetails(OleDbDataReader readerInstance, int offset, bool loadClearanceData) =>
            new PersonnelDetails {
                NRIC = readerInstance[offset].ToString(),
                Rank = readerInstance[offset + 1].ToString().ToUpper(),
                FullName = readerInstance[offset + 2].ToString().ToUpper(),
                Contact = readerInstance[offset + 3].ToString().ToUpper(),
                UnitOrCompany = readerInstance[offset + 4].ToString().ToUpper(),
                VehicleNum = readerInstance[offset + 5].ToString().ToUpper(),
                IsBasePersonnel = readerInstance.GetBoolean(offset + 6),
                AllowedToEscort = readerInstance.GetBoolean(offset + 7),
                IsBanStatus = readerInstance.GetBoolean(offset + 8),
                Remarks = readerInstance[offset + 9].ToString().ToUpper(),
                ListofClearance = loadClearanceData ? GetVisitorClearanceData(readerInstance[offset].ToString()) : null,
                IsDirty = false
            };
        public VisitorRecord ReadToVisitorRecord(OleDbDataReader readerInstance) {
            return new VisitorRecord {
                Visitor = ReadToPersonnelDetails(readerInstance, 0, false),
                Escort =
                    (readerInstance[10].ToString() == "NA") ? null : ReadToPersonnelDetails(readerInstance, 10, false),
                StartTime = readerInstance.GetDateTime(20),
                EndTime = (readerInstance.GetBoolean(26)) ? (DateTime?)(null) : readerInstance.GetDateTime(21),
                PersonnelPass = readerInstance[22].ToString().ToUpper(),
                VehiclePass = readerInstance[23].ToString().ToUpper(),
                VehicleNum = readerInstance[24].ToString().ToUpper(),
                LockerNum = readerInstance[25].ToString().ToUpper(),
                InCampStatus = readerInstance.GetBoolean(26)
            };
        }

        #endregion

        #region [DataBase Interaction]

        #region [DASH BOARD]

        [HandleProcessCorruptedStateExceptions]
        public ObservableCollection<VisitorRecord> GetVisitorRecords() {
            var visitorRecord = new ObservableCollection<VisitorRecord>();
            var vmsDatabase = new OleDbConnection(_connectionString);

            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    CommandText =
                        "SELECT " +
                        "Visitor.NRIC, Visitor.RANK, Visitor.FULLNAME, Visitor.CONTACT, Visitor.COMPANY, Visitor.VEHICLENUM, Visitor.IsMilitaryPersonnel, Visitor.AllowToEscort, Visitor.BAN, Visitor.Remarks," +
                        "Escort.NRIC, Escort.RANK, Escort.FULLNAME, Escort.CONTACT, Escort.COMPANY, Escort.VEHICLENUM, Escort.IsMilitaryPersonnel, Escort.AllowToEscort, Escort.BAN, Escort.Remarks," +
                        "Record.TIMEIN, Record.TIMEOUT, Record.PASSNO, Record.VEHPASSNO, Record.VEHNUM, Record.LOCKERKEY, Record.INCAMPSTATUS " +
                        "FROM VisitorRecords As Record, PersonnelDetails As Visitor, PersonnelDetails As Escort " +
                        "WHERE Visitor.NRIC=Record.NRIC AND Escort.NRIC=Record.ESCORTNRIC AND Record.INCAMPSTATUS=TRUE " +
                        "ORDER BY Record.TIMEIN DESC",
                    Connection = vmsDatabase
                };

                var reader = comm.ExecuteReader();
                if (reader != null && reader.HasRows)
                {
                    while (reader.Read()) { visitorRecord.Add(ReadToVisitorRecord(reader)); }
                }

                comm.Dispose();
                reader?.Close();
            } catch (Exception ex) {
                MessageBox.Show(
                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally
            {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return visitorRecord;
        }

        [HandleProcessCorruptedStateExceptions]
        public ObservableCollection<VisitorRecord> GetVisitorHistory(string nric) {
            var visitorRecord = new ObservableCollection<VisitorRecord>();
            var vmsDatabase   = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    CommandText =
                        "SELECT " +
                        "Visitor.NRIC, Visitor.RANK, Visitor.FULLNAME, Visitor.CONTACT, Visitor.COMPANY, Visitor.VEHICLENUM, Visitor.IsMilitaryPersonnel, Visitor.AllowToEscort, Visitor.BAN, Visitor.Remarks," +
                        "Escort.NRIC, Escort.RANK, Escort.FULLNAME, Escort.CONTACT, Escort.COMPANY, Escort.VEHICLENUM, Escort.IsMilitaryPersonnel, Escort.AllowToEscort, Escort.BAN, Escort.Remarks," +
                        "Record.TIMEIN, Record.TIMEOUT, Record.PASSNO, Record.VEHPASSNO, Record.VEHNUM, Record.LOCKERKEY, Record.INCAMPSTATUS " +
                        "FROM VisitorRecords As Record, PersonnelDetails As Visitor, PersonnelDetails As Escort " +
                        "WHERE (Record.NRIC=@_visitornric OR Record.ESCORTNRIC=@_escortnric) " +
                        "AND Visitor.NRIC=Record.NRIC AND Escort.NRIC=Record.ESCORTNRIC",
                    Connection = vmsDatabase
                };
                comm.Parameters.AddWithValue("@_visitornric", nric);
                comm.Parameters.AddWithValue("@_escortnric", nric);

                var reader = comm.ExecuteReader();
                if(reader != null && reader.HasRows) {
                    while(reader.Read()) { visitorRecord.Add(ReadToVisitorRecord(reader)); }
                }

                comm.Dispose();
                reader?.Close();
            } catch(Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return visitorRecord;
        }

        [HandleProcessCorruptedStateExceptions]
        public ObservableCollection<VisitorRecord> GetVisitorPassHistory(string vistorPassId) {
            var visitorRecord = new ObservableCollection<VisitorRecord>();
            var vmsDatabase = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    CommandText =
                        "SELECT " +
                        "Visitor.NRIC, Visitor.RANK, Visitor.FULLNAME, Visitor.CONTACT, Visitor.COMPANY, Visitor.VEHICLENUM, Visitor.IsMilitaryPersonnel, Visitor.AllowToEscort, Visitor.BAN, Visitor.Remarks," +
                        "Escort.NRIC, Escort.RANK, Escort.FULLNAME, Escort.CONTACT, Escort.COMPANY, Escort.VEHICLENUM, Escort.IsMilitaryPersonnel, Escort.AllowToEscort, Escort.BAN, Escort.Remarks," +
                        "Record.TIMEIN, Record.TIMEOUT, Record.PASSNO, Record.VEHPASSNO, Record.VEHNUM, Record.LOCKERKEY, Record.INCAMPSTATUS " +
                        "FROM VisitorRecords As Record, PersonnelDetails As Visitor, PersonnelDetails As Escort " +
                        "WHERE Record.PASSNO=@_visitorpassID " +
                        "AND Visitor.NRIC=Record.NRIC AND Escort.NRIC=Record.ESCORTNRIC",
                    Connection = vmsDatabase
                };
                comm.Parameters.AddWithValue("@_visitorpassID", vistorPassId);

                var reader = comm.ExecuteReader();
                if(reader != null && reader.HasRows) {
                    while(reader.Read()) { visitorRecord.Add(ReadToVisitorRecord(reader)); }
                }

                comm.Dispose();
                reader?.Close();
            } catch(Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return visitorRecord;
        }

        [HandleProcessCorruptedStateExceptions]
        public IList<VisitorRecord> BookOutVisitorRecords(IList<VisitorRecord> recordstoBookOut) {

            var timeoutDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var vmsDatabase  = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var trans = vmsDatabase.BeginTransaction();
                var comm = new OleDbCommand {Connection = vmsDatabase, Transaction = trans };

                foreach (var indexRecord in recordstoBookOut)
                {
                    indexRecord.InCampStatus = false;
                    comm.CommandText = "UPDATE VisitorRecords " + "SET INCAMPSTATUS=FALSE, TIMEOUT=@_timeout " +
                                       "WHERE INCAMPSTATUS=TRUE AND NRIC=@_nric AND ESCORTNRIC=@_escortnric";

                    comm.Parameters.AddWithValue("@_timeout", timeoutDateTimeStr);
                    comm.Parameters.AddWithValue("@_nric", indexRecord.NRIC);
                    comm.Parameters.AddWithValue("@_escortnric", indexRecord.EscortNRIC);

                    comm.ExecuteNonQuery();
                    comm.Parameters.Clear();
                }

                trans.Commit();

                comm.Dispose();
                trans.Dispose();
            } catch(Exception ex) {
                MessageBox.Show(
                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return GetVisitorRecords();
        }

        #endregion

        #region [Book In Form]

        [HandleProcessCorruptedStateExceptions]
        public IList<Clearance> GetVisitorClearanceData(string nric) {
            if(nric.Length < 9)
                nric = nric.PadRight(9, '-');
            else if(nric.Length > 9)
                nric = nric.Substring(0, 9);

            var vmsDatabase = new OleDbConnection(_connectionString);
            var listofClearance = new ObservableCollection<Clearance>();
            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    CommandText = "SELECT AREA, STARTDATE, EXPIRYDATE, REMARKS FROM Clearances WHERE NRIC=@_nric",
                    Connection = vmsDatabase
                };
                comm.Parameters.AddWithValue("@_nric", nric);

                var reader = comm.ExecuteReader();
                if(reader != null && reader.HasRows) {
                    while(reader.Read()) {
                        listofClearance.Add(
                            new Clearance {
                                NRIC = nric,
                                AreaAccess = FromStringToClearanceLevelEnum(reader.GetString(0)),
                                StartDate = reader.GetDateTime(1),
                                EndDate = reader.GetDateTime(2),
                                ClearanceDetails = reader.GetString(3)
                            });
                    }
                }

                comm.Dispose();
                reader?.Close();
            } catch {
                // ignored
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return listofClearance;
        }

        [HandleProcessCorruptedStateExceptions]
        public PersonnelDetails GetVisitorPersonnelDetails(string nric) {
            if (nric.Length < 9)
                nric = nric.PadRight(9, '-');
            else if(nric.Length > 9)
                nric = nric.Substring(0, 9);

            PersonnelDetails returnVal = null;
            var vmsDatabase  = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    Connection = vmsDatabase,
                    CommandText =
                        "SELECT NRIC, RANK, FULLNAME, CONTACT, COMPANY, VEHICLENUM, IsMilitaryPersonnel, AllowToEscort, BAN, REMARKS " +
                        "FROM PersonnelDetails WHERE NRIC=@_nric"
                };
                comm.Parameters.AddWithValue("@_nric", nric);

                var reader = comm.ExecuteReader();
                if (reader != null && reader.HasRows && reader.Read())
                    returnVal = ReadToPersonnelDetails(reader, 0, true);

                comm.Dispose();
                reader?.Close();
            } catch (Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return returnVal ?? new PersonnelDetails {
                NRIC            = nric,
                Rank            = "MR",
                FullName        = "",
                Contact         = "",
                UnitOrCompany   = "",
                VehicleNum      = "",
                Remarks         = "",
                IsBasePersonnel = false,
                AllowedToEscort = false,
                IsBanStatus     = false,
                ListofClearance = new ObservableCollection<Clearance>(),
                IsDirty         = false
            };
        }

        [HandleProcessCorruptedStateExceptions]
        public ObservableCollection<PersonnelDetails> SearchEscortPersonnelDetails(string nric) {
            var vmsDatabase = new OleDbConnection(_connectionString);
            var listofEscort = new ObservableCollection<PersonnelDetails>();
            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    Connection = vmsDatabase,
                    CommandText =
                        "SELECT " +
                        "NRIC, RANK, FULLNAME, CONTACT, COMPANY, VEHICLENUM, IsMilitaryPersonnel, AllowToEscort, BAN, REMARKS " +
                        "FROM PersonnelDetails WHERE (NRIC LIKE @_nric OR FULLNAME LIKE @_name OR COMPANY LIKE @_company)"
                };

                comm.Parameters.AddWithValue("@_nric", $"%{nric}%");
                comm.Parameters.AddWithValue("@_name", $"%{nric}%");
                comm.Parameters.AddWithValue("@_company", $"%{nric}%");

                var reader = comm.ExecuteReader();
                if(reader != null && reader.HasRows)
                    while (reader.Read())
                        listofEscort.Add(ReadToPersonnelDetails(reader, 0, true));

                comm.Dispose();
                reader?.Close();
            } catch(Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            var targetNric = (nric.Length == 9)
                ? nric
                : (nric.Length < 9)
                    ? nric.PadRight(9,'-')
                    : nric.Substring(0, 9);

            if (nric.Length > 7 && (listofEscort.Count == 0 || listofEscort.All(x => x.NRIC != targetNric))) {
                listofEscort.Add(new PersonnelDetails {
                    NRIC            = targetNric,
                    Rank            = "MR",
                    FullName        = "",
                    Contact         = "",
                    UnitOrCompany   = "",
                    VehicleNum      = "",
                    Remarks         = "",
                    IsBasePersonnel = false,
                    AllowedToEscort = false,
                    IsBanStatus     = false,
                    ListofClearance = new ObservableCollection<Clearance>(),
                    IsDirty         = false
                });
            }

            return listofEscort;
        }

        [HandleProcessCorruptedStateExceptions]
        public bool UpdateVisitorPersonnelDetails(PersonnelDetails personnel) {

            if(personnel == null || !personnel.IsDirty)
                return false;

            personnel.Sterilise();

            var actionSuccessful = false;
            var vmsDatabase      = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();

                //Update Personnel details
                var trans = vmsDatabase.BeginTransaction();
                var comm = new OleDbCommand {
                    CommandText = "UPDATE PersonnelDetails " +
                                  "SET RANK=@_rank, FULLNAME=@_fullname, CONTACT=@_contact, COMPANY=@_company, VEHICLENUM=@_vehicleNum, " +
                                  "IsMilitaryPersonnel=@_isBasePersonnel, AllowToEscort=@_isEscort, BAN=@_isBan, " +
                                  "REMARKS=@_remarks, LASTEDIT=@_currentyear " +
                                  "WHERE NRIC=@_nric",
                    Connection = vmsDatabase,
                    Transaction = trans
                };
                comm.Parameters.AddWithValue("@_rank", personnel.Rank);
                comm.Parameters.AddWithValue("@_fullname", personnel.FullName);
                comm.Parameters.AddWithValue("@_contact", personnel.Contact);
                comm.Parameters.AddWithValue("@_company", personnel.UnitOrCompany);
                comm.Parameters.AddWithValue("@_vehicleNum", personnel.VehicleNum);
                comm.Parameters.AddWithValue("@_isBasePersonnel", personnel.IsBasePersonnel);
                comm.Parameters.AddWithValue("@_isEscort", personnel.AllowedToEscort);
                comm.Parameters.AddWithValue("@_isBan", personnel.IsBanStatus);
                comm.Parameters.AddWithValue("@_remarks", personnel.Remarks);
                comm.Parameters.AddWithValue("@_currentyear", DateTime.Now.Year);
                comm.Parameters.AddWithValue("@_nric", personnel.NRIC);

                //Insert Personnel details (if not in record)
                if(comm.ExecuteNonQuery() == 0) {
                    comm.Parameters.Clear();
                    comm.CommandText =
                        "INSERT INTO PersonnelDetails(NRIC, RANK, FULLNAME, CONTACT, COMPANY, VEHICLENUM, IsMilitaryPersonnel, AllowToEscort, BAN, REMARKS, LASTEDIT) " +
                        "VALUES(@_nric, @_rank, @_fullname, @_contact, @_company, @_vehicleNum, @_isBasePersonnel, @_isEscort, @_isBan, @_remarks, @_currentyear)";
                    
                    comm.Parameters.AddWithValue("@_nric", personnel.NRIC);
                    comm.Parameters.AddWithValue("@_rank", personnel.Rank);
                    comm.Parameters.AddWithValue("@_fullname", personnel.FullName);
                    comm.Parameters.AddWithValue("@_contact", personnel.Contact);
                    comm.Parameters.AddWithValue("@_company", personnel.UnitOrCompany);
                    comm.Parameters.AddWithValue("@_vehicleNum", personnel.VehicleNum);
                    comm.Parameters.AddWithValue("@_isBasePersonnel", personnel.IsBasePersonnel);
                    comm.Parameters.AddWithValue("@_isEscort", personnel.AllowedToEscort);
                    comm.Parameters.AddWithValue("@_isBan", personnel.IsBanStatus);
                    comm.Parameters.AddWithValue("@_remarks", personnel.Remarks);
                    comm.Parameters.AddWithValue("@_currentyear", DateTime.Now.Year.ToString());

                    comm.ExecuteNonQuery();
                }
                comm.Parameters.Clear();

                //Delete all existing personnel clearance
                comm.CommandText = "DELETE FROM Clearances WHERE NRIC=@_nric";
                comm.Parameters.AddWithValue("@_nric", personnel.NRIC);
                comm.ExecuteNonQuery();
                comm.Parameters.Clear();

                //Update Personnel clearance
                foreach (var currentclearance in personnel.ListofClearance)
                {
                    comm.CommandText = "INSERT INTO Clearances (NRIC, AREA, STARTDATE, EXPIRYDATE, REMARKS) " +
                                       "VALUES(@_nric, @_area, @_start, @_end, @_remarks)";

                    comm.Parameters.AddWithValue("@_nric", personnel.NRIC);
                    comm.Parameters.AddWithValue("@_area", FromClearanceLevelEnumToString(currentclearance.AreaAccess));
                    comm.Parameters.AddWithValue("@_start", currentclearance.StartDate.ToString("yyyy-MM-dd"));
                    comm.Parameters.AddWithValue("@_end", currentclearance.EndDate.ToString("yyyy-MM-dd"));
                    comm.Parameters.AddWithValue("@_remarks", currentclearance.ClearanceDetails);

                    comm.ExecuteNonQuery();
                    comm.Parameters.Clear();
                }
                trans.Commit();

                comm.Dispose();
                trans.Dispose();

                personnel.IsDirty = false;
                actionSuccessful = true;
            } catch(Exception ex) {
                // ignored

                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return actionSuccessful;
        }
        
        [HandleProcessCorruptedStateExceptions]
        public bool BookInAllPersonnel(IList<VisitorRecord> records, PersonnelDetails escort) {
            var successful = false;

            //Update Escort personnel details
            UpdateVisitorPersonnelDetails(escort);
            foreach(var record in records) {

                if (record.PassEntry != null)
                {
                    record.PersonnelPass = record.PassEntry.PersonnelPass.Trim();
                    record.VehiclePass = record.PassEntry.VehiclePass.Trim();
                    record.LockerNum = record.PassEntry.LockerNum.Trim();
                    record.VehicleNum = record.Visitor.VehicleNum;

                    if (string.IsNullOrEmpty(record.PersonnelPass))
                        record.PersonnelPass = "NA";
                }
                //Read and update all personnel details
                UpdateVisitorPersonnelDetails(record.Visitor);

                //Update Escort
                record.Escort = escort;
            }

            var timeinDateTimeStr  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var vmsDatabase        = new OleDbConnection(_connectionString);
            try
            {
                vmsDatabase.Open();
                var trans = vmsDatabase.BeginTransaction();
                var comm  = new OleDbCommand { Connection = vmsDatabase, Transaction = trans };

                foreach (var record in records)
                {
                    comm.CommandText =
                        "INSERT INTO VisitorRecords (NRIC, ESCORTNRIC, TIMEIN, TIMEOUT, PASSNO, VEHPASSNO, LOCKERKEY, VEHNUM, INCAMPSTATUS) " +
                        "VALUES (@_nric, @_escortnric, @_timein, @_timeout, @_passno, @_vehpassno, @_locker, @_vehnum, TRUE)";

                    //Add Book In Record
                    comm.Parameters.AddWithValue("@_nric", record.NRIC);
                    comm.Parameters.AddWithValue("@_escortnric", escort?.NRIC ?? "NA");
                    comm.Parameters.AddWithValue("@_timein", timeinDateTimeStr);
                    comm.Parameters.AddWithValue("@_timeout", "2018-01-01 00:00:00");
                    comm.Parameters.AddWithValue("@_passno", record.PersonnelPass ?? "NA");
                    comm.Parameters.AddWithValue("@_vehpassno", record.VehiclePass ?? "");
                    comm.Parameters.AddWithValue("@_locker", record.LockerNum ?? "");
                    comm.Parameters.AddWithValue("@_vehnum", record.VehicleNum ?? "");

                    comm.ExecuteNonQuery();
                    comm.Parameters.Clear();
                }

                trans.Commit();

                comm.Dispose();
                trans.Dispose();
                successful = true;
            } catch (Exception ex)
            {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally
            {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return successful;
        }

        #endregion
        
        #region [Book Out History Form]

        [HandleProcessCorruptedStateExceptions]
        public ObservableCollection<VisitorRecord> GetBookOutHistoryRecords(DateTime startDateTime, DateTime endDateTime) {
            var visitorRecord = new ObservableCollection<VisitorRecord>();
            var vmsDatabase = new OleDbConnection(_connectionString);

            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    CommandText =
                        "SELECT " +
                        "Visitor.NRIC, Visitor.RANK, Visitor.FULLNAME, Visitor.CONTACT, Visitor.COMPANY, Visitor.VEHICLENUM, Visitor.IsMilitaryPersonnel, Visitor.AllowToEscort, Visitor.BAN, Visitor.Remarks," +
                        "Escort.NRIC, Escort.RANK, Escort.FULLNAME, Escort.CONTACT, Escort.COMPANY, Escort.VEHICLENUM, Escort.IsMilitaryPersonnel, Escort.AllowToEscort, Escort.BAN, Escort.Remarks," +
                        "Record.TIMEIN, Record.TIMEOUT, Record.PASSNO, Record.VEHPASSNO, Record.VEHNUM, Record.LOCKERKEY, Record.INCAMPSTATUS " +
                        "FROM VisitorRecords As Record, PersonnelDetails As Visitor, PersonnelDetails As Escort " +
                        "WHERE Visitor.NRIC=Record.NRIC AND Escort.NRIC=Record.ESCORTNRIC AND Record.INCAMPSTATUS=FALSE " +
                        "AND TIMEOUT >= @_startDateTime AND TIMEOUT <= @_endDateTime " +
                        "ORDER BY Record.TIMEOUT DESC",
                    Connection = vmsDatabase
                };
                comm.Parameters.AddWithValue("@_startDateTime", $"{startDateTime:yyyy-MM-dd} 12:00:00 AM");
                comm.Parameters.AddWithValue("@_endDateTime", $"{endDateTime:yyyy-MM-dd} 11:59:59 PM");

                var reader = comm.ExecuteReader();
                if (reader != null && reader.HasRows)
                {
                    while (reader.Read()) { visitorRecord.Add(ReadToVisitorRecord(reader)); }
                }

                comm.Dispose();
                reader?.Close();
            } catch (Exception ex) {
                MessageBox.Show(
                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally
            {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return visitorRecord;
        }

        #endregion

        #region [Pass Management Form]

        [HandleProcessCorruptedStateExceptions]
        public ObservableCollection<VisitorPass> GetVisitorPasses() {
            var listofVisitorPasses = new ObservableCollection<VisitorPass>();
            var vmsDatabase   = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var comm = new OleDbCommand {
                    CommandText =
                        "SELECT PASSNUM, AREA, STATUS, RequireEscort, OrderOfPreference " +
                        "FROM VisitorPassManagement ORDER BY OrderOfPreference ASC",
                    Connection = vmsDatabase
                };

                int currentIndex = 1;
                var reader = comm.ExecuteReader();
                if(reader != null && reader.HasRows) {
                    while (reader.Read())
                    {
                        listofVisitorPasses.Add(new VisitorPass {
                            PassId        = reader[0].ToString().ToUpper(),
                            AreaAccess    = FromStringToClearanceLevelEnum(reader[1].ToString().ToUpper()),
                            PassCondition = FromStringToPassConditionEnum(reader[2].ToString().ToUpper()),
                            RequireEscort = reader.GetBoolean(3),
                            OrderOfPerference = currentIndex,
                            IsDirty = false
                        });
                        currentIndex++;
                    }
                }

                comm.Dispose();
                reader?.Close();
            } catch(Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return listofVisitorPasses;
        }

        [HandleProcessCorruptedStateExceptions]
        public bool UpdateVisitorPasses(IList<VisitorPass> visitorPasses) {
            var actionSuccessful = false;
            var vmsDatabase = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();

                //Update Personnel details
                var trans = vmsDatabase.BeginTransaction();
                var comm = new OleDbCommand {
                    CommandText = "DELETE * FROM VisitorPassManagement",
                    Connection = vmsDatabase,
                    Transaction = trans
                };

                //Delete all entries in table
                comm.ExecuteNonQuery();

                var count = 0;
                foreach(var currentPass in visitorPasses) {
                    count++;
                    currentPass.OrderOfPerference = count;
                    comm.CommandText =
                        "INSERT INTO VisitorPassManagement(PASSNUM, AREA, STATUS, RequireEscort, OrderOfPreference) " +
                        "VALUES(@_passnum, @_area, @_status, @_requireEscort, @_perference)";

                    comm.Parameters.AddWithValue("@_passnum", currentPass.PassId);
                    comm.Parameters.AddWithValue("@_area", currentPass.AreaAccessString);
                    comm.Parameters.AddWithValue("@_status", currentPass.PassConditionString);
                    comm.Parameters.AddWithValue("@_requireEscort", currentPass.RequireEscort);
                    comm.Parameters.AddWithValue("@_perference", currentPass.OrderOfPerference);

                    comm.ExecuteNonQuery();
                    comm.Parameters.Clear();

                    currentPass.IsDirty = false;
                }
                trans.Commit();

                comm.Dispose();
                trans.Dispose();
                actionSuccessful = true;
            } catch(Exception ex) {
                // ignored

                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return actionSuccessful;
        }

        #endregion

        #region [Bulk Clearance Management Form]

        [HandleProcessCorruptedStateExceptions]
        public bool UpdateBulkClearances(IList<BluePassBulkClearance> bluePassClearances) {
            var successful = false;
            var todayDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd");
            var lastNric = "";

            //Combine all bluePassClearances by NRIC
            IEnumerable<IGrouping<string, BluePassBulkClearance>> listofClearances =
                bluePassClearances
                    .Select(x => x.Sterilise())
                    .Where(x => x.IsValid)
                    .GroupBy(
                        clearance => clearance.NRIC,
                        clearance => clearance);

            var vmsDatabase = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var trans = vmsDatabase.BeginTransaction();
                var comm = new OleDbCommand { Connection = vmsDatabase, Transaction = trans };

                foreach(var bluePass in listofClearances)
                {
                    var highestBluePassClearanceEnum = ClearanceLevelEnum.None;
                    BluePassBulkClearance highestBluePassClearance = null;
                    lastNric = bluePass.Key;

                    //Delete all existing personnel clearance
                    comm.CommandText = "DELETE FROM Clearances WHERE NRIC=@_nric";
                    comm.Parameters.AddWithValue("@_nric", bluePass.Key);
                    comm.ExecuteNonQuery();
                    comm.Parameters.Clear();

                    foreach (var clearanceDetails in bluePass.ToArray()) {
                        //Update Personnel clearance
                        comm.CommandText = "INSERT INTO Clearances (NRIC, AREA, STARTDATE, EXPIRYDATE, REMARKS) " +
                                           "VALUES(@_nric, @_area, @_start, @_end, @_remarks)";

                        comm.Parameters.AddWithValue("@_nric", bluePass.Key);
                        comm.Parameters.AddWithValue("@_area", FromClearanceLevelEnumToString(clearanceDetails.AreaAccess));
                        comm.Parameters.AddWithValue("@_start", todayDateTimeStr);
                        comm.Parameters.AddWithValue("@_end", clearanceDetails.EndDate.ToString("yyyy-MM-dd"));
                        comm.Parameters.AddWithValue("@_remarks", clearanceDetails.ClearanceDetails);
                        comm.ExecuteNonQuery();
                        comm.Parameters.Clear();

                        if ((byte)clearanceDetails.AreaAccess > (byte)highestBluePassClearanceEnum)
                            highestBluePassClearance = clearanceDetails;
                    }

                    if (highestBluePassClearance == null)
                        continue;

                    //Update Personnel Remarks
                    comm.CommandText = "UPDATE PersonnelDetails " +
                                       "SET REMARKS=@_remarks, LASTEDIT=@_currentyear " +
                                       "WHERE NRIC=@_nric";

                    comm.Parameters.AddWithValue("@_remarks", highestBluePassClearance.ClearanceDetails);
                    comm.Parameters.AddWithValue("@_currentyear", DateTime.Now.Year);
                    comm.Parameters.AddWithValue("@_nric", highestBluePassClearance.NRIC);

                    //Insert Personnel details (if not in record)
                    if(comm.ExecuteNonQuery() == 0) {
                        comm.Parameters.Clear();
                        comm.CommandText =
                            "INSERT INTO PersonnelDetails(NRIC, RANK, FULLNAME, CONTACT, COMPANY, VEHICLENUM, IsMilitaryPersonnel, AllowToEscort, BAN, REMARKS, LASTEDIT) " +
                            "VALUES(@_nric, @_rank, @_fullname, \"\", @_company, \"\", FALSE, FALSE, FALSE, @_remarks, @_currentyear)";

                        comm.Parameters.AddWithValue("@_nric", highestBluePassClearance.NRIC);
                        comm.Parameters.AddWithValue("@_rank", highestBluePassClearance.Rank);
                        comm.Parameters.AddWithValue("@_fullname", highestBluePassClearance.FullName);
                        comm.Parameters.AddWithValue("@_company", highestBluePassClearance.Company);
                        comm.Parameters.AddWithValue("@_remarks", highestBluePassClearance.ClearanceDetails);
                        comm.Parameters.AddWithValue("@_currentyear", DateTime.Now.Year.ToString());

                        comm.ExecuteNonQuery();
                    }
                    comm.Parameters.Clear();
                }

                trans.Commit();

                comm.Dispose();
                trans.Dispose();
                successful = true;
            } catch(Exception ex) {
                MessageBox.Show(
                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message + "\r\n\r\nNRIC: " + lastNric,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }

            return successful;
        }

        #endregion

        #region [DataBase Management Functions]

        public void CompactDataBase(bool deleteOutdatedVisitorRecords) {

            var currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd");
            var previousDateTimeStr = DateTime.Now.AddYears(-2).ToString("yyyy-MM-dd");
            var vmsDatabase        = new OleDbConnection(_connectionString);
            try {
                vmsDatabase.Open();
                var trans = vmsDatabase.BeginTransaction();

                var comm = new OleDbCommand {
                    Connection  = vmsDatabase,
                    Transaction = trans,
                    CommandText = "DELETE * FROM VisitorRecords WHERE TIMEOUT <= @_expiryDate AND NOT INCAMPSTATUS=TRUE"
                };
                comm.Parameters.AddWithValue("@_expiryDate", previousDateTimeStr);

                if(deleteOutdatedVisitorRecords)
                    comm.ExecuteNonQuery();


                comm.Parameters.Clear();
                comm.CommandText = "DROP TABLE [TempClearanceTable]";
                try {
                    comm.ExecuteNonQuery();
                } catch (Exception) { /* ignored */ }

                comm.CommandText = "SELECT DISTINCT * INTO TempClearanceTable FROM Clearances WHERE EXPIRYDATE >= @_expiryDate AND NOT  AREA='NONE'";
                comm.Parameters.AddWithValue("@_expiryDate", currentDateTimeStr);
                comm.ExecuteNonQuery();

                comm.Parameters.Clear();
                comm.CommandText = "DELETE FROM Clearances";
                comm.ExecuteNonQuery();

                comm.CommandText = "INSERT INTO Clearances SELECT * FROM TempClearanceTable";
                comm.ExecuteNonQuery();

                comm.CommandText = "DROP TABLE [TempClearanceTable]";
                comm.ExecuteNonQuery();

                trans.Commit();
                System.Diagnostics.Process.Start(_locationOfMicrosoftAccess, $"\"{DataBaseLocation}\" /compact")?.WaitForExit(500);

                comm.Dispose();
                trans.Dispose();
            } catch(Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred when connecting to DataBase!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } finally {
                vmsDatabase.Close();
                vmsDatabase.Dispose();
            }
        }

        public void OverrideDataBaseWithDefault(bool shutdownIfDeclined) {
            var msgResult = MessageBox.Show(
                "You are about to reset the Visitor Management DataBase.\r\n" +
                "All records will be lost! This action is irreversible.\r\n\r\n" +
                "Do you wish to proceed with resetting the Data Base?",
                "Reset Visitor Management DataBase",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if(msgResult != MessageBoxResult.Yes) {
                if(shutdownIfDeclined)
                    Environment.Exit(0);
                else
                    return;
            }

            try
            {
                using (var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("SLCMS.Assets.SLVMS_DB_Default.accdb"))
                {
                    using (var fileStream = new FileStream(DataBaseLocation, FileMode.Create))
                    {
                        if (stream != null)
                            for (var i = 0; i < stream.Length; i++) { fileStream.WriteByte((byte) stream.ReadByte()); }

                        fileStream.Close();
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(

                    "Please ensure the database is not used by another process\r\n\r\n" + ex.Message,
                    "An unexpected error occurred!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                if(shutdownIfDeclined)
                    Environment.Exit(0);
                else
                    return;
            }

            if(RunConnectionTest()) {
                MessageBox.Show(
                    
                    "Visitor Management DataBase has been reset successfully.",
                    "Operation Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            } else {
                MessageBox.Show(
                    
                    "Visitor Management DataBase could not be reset.",
                    "Operation Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                if(shutdownIfDeclined)
                    Environment.Exit(1);
            }
        }

        #endregion

        #endregion
    }
}