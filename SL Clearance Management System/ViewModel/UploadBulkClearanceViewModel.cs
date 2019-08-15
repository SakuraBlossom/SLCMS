using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.ViewModel {
    public class UploadBulkClearanceViewModel : INotifyPropertyChanged {
        private TabControl _bulkClearanceUploadTabControl;
        private ICommand _proceedToParseClearancesScreenCommand;
        private ICommand _proceedToCommitChangesScreenCommand;
        private ICommand _commitChangesCommand;
        private IList<BluePassBulkClearance> _listofBluePassBulkClearance;
        private int _numberofValidClearances;
        private int _numberofInvalidClearances;

        #region [Properties]

        #endregion

        #region [Visitor Selection Commands]

        public ICommand ProceedToParseClearancesScreenCommand {
            get => _proceedToParseClearancesScreenCommand;
            set {
                _proceedToParseClearancesScreenCommand = value;
                RaisePropertyChangedEvent(nameof(ProceedToParseClearancesScreenCommand));
            }
        }
        public ICommand ProceedToCommitChangesScreenCommand {
            get => _proceedToCommitChangesScreenCommand;
            set {
                _proceedToCommitChangesScreenCommand = value;
                RaisePropertyChangedEvent(nameof(ProceedToCommitChangesScreenCommand));
            }
        }
        public ICommand CommitChangesCommand {
            get => _commitChangesCommand;
            set {
                _commitChangesCommand = value;
                RaisePropertyChangedEvent(nameof(CommitChangesCommand));
            }
        }
        public IList<BluePassBulkClearance> ListofBluePassBulkClearances {
            get => _listofBluePassBulkClearance;
            set {
                _listofBluePassBulkClearance = value;
                RaisePropertyChangedEvent(nameof(ListofBluePassBulkClearances));
            }
        }
        public int NumberofValidClearances {
            get => _numberofValidClearances;
            set {
                _numberofValidClearances = value;
                RaisePropertyChangedEvent(nameof(NumberofValidClearances));
            }
        }
        public int NumberofInvalidClearances {
            get => _numberofInvalidClearances;
            set {
                _numberofInvalidClearances = value;
                RaisePropertyChangedEvent(nameof(NumberofInvalidClearances));
            }
        }

        #endregion

        public UploadBulkClearanceViewModel(TabControl bulkClearanceUploadTabControl) {
            _bulkClearanceUploadTabControl = bulkClearanceUploadTabControl;
            ListofBluePassBulkClearances = null;

            ProceedToParseClearancesScreenCommand = new RelayCommand(
                delegate {
                    var openExcelFileDialog = new OpenFileDialog {
                        InitialDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "c:\\",
                        Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"
                    };

                    if (openExcelFileDialog.ShowDialog() == true) {
                        _ = TryReadExcelSheet(openExcelFileDialog.FileName);
                    }
                });
            ProceedToCommitChangesScreenCommand = new RelayCommand(
                delegate {
                    foreach(var currentclearance in ListofBluePassBulkClearances) { currentclearance.Sterilise(); }

                    NumberofValidClearances = ListofBluePassBulkClearances.Count(x => x.IsValid);
                    NumberofInvalidClearances = ListofBluePassBulkClearances.Count(x => !x.IsValid);
                    _bulkClearanceUploadTabControl.SelectedIndex = 2;
                });
            CommitChangesCommand = new RelayCommand(
                async delegate {

                    Global.MainViewModel.IsLoading = true;
                    var successful = await Task.Run(
                        () => Global.DataBase.UpdateBulkClearances(ListofBluePassBulkClearances));

                    if(successful) {
                        Global.MainViewModel.ViewModelDashBoard.RefreshVisitorDataCommand.Execute(null);
                        _bulkClearanceUploadTabControl.SelectedIndex = 0;
                        Global.ForceGarbageCollector();

                        MessageBox.Show(
                            "Clearance data successfully committed to database",
                            "Operation successful",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    Global.MainViewModel.IsLoading = false;
                });


        }

        public async Task TryReadExcelSheet(string fileLocation) {
            if(!File.Exists(fileLocation)) {
                MessageBox.Show(
                    $"Could not open file located at {fileLocation}",
                    "Failed to open Excel file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            Global.MainViewModel.IsLoading = true;
            ListofBluePassBulkClearances = await Task.Run(() => ParseExcelSheetHelper(fileLocation));
            Global.MainViewModel.IsLoading = false;

            _bulkClearanceUploadTabControl.SelectedIndex = (ListofBluePassBulkClearances.Count > 0) ? 1 : 0;
            Global.ForceGarbageCollector();
        }

        public IList<BluePassBulkClearance> ParseExcelSheetHelper(string fileLocation) {
            var resListofBluePassBulkClearances = new ObservableCollection<BluePassBulkClearance>();
            var firstTimeShowingError = false;
            var suppressErrorMessages = false;
            var exceptionThrown = false;

            var matchTargetColumn = -1;
            var topLeftActiveRangeColumn = -1;
            var topLeftActiveRangeRow = -1;
            var bottomRightActiveRangeColumn = -1;

            var oleDbcon = new OleDbConnection($"Provider=\'{Global.DataBase.OleDbProvider}\';Data Source=\'{fileLocation}\';Extended Properties=\"Excel 12.0 Xml;HDR=NO\"");
            try {
                oleDbcon.Open();
                var dataTable = oleDbcon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if(dataTable == null || dataTable.Rows.Count == 0)
                    throw new Exception("Unable to open Excel Sheet");

                //Read all Tables
                for(var index = 0; index < dataTable.Rows.Count; index++) {
                    var sheetName = dataTable.Rows[index]["TABLE_NAME"].ToString();
                    if(sheetName.Length > 2 && sheetName.StartsWith("\'") && sheetName.EndsWith("\'"))
                        sheetName = sheetName.Substring(1, sheetName.Length - 2);

                    sheetName = sheetName.Substring(0, sheetName.Length - 1);

                    //Table Active Range has been Identified
                    if(topLeftActiveRangeRow != -1)
                        continue;
                    
                    var xlsData = new DataTable();
                    try {
                        var dbAdapter = new OleDbDataAdapter($"SELECT * FROM [{sheetName}$A1:Z10]", oleDbcon);
                        dbAdapter.Fill(xlsData);
                        dbAdapter.Dispose();

                        //Find Distinct Header "CLEARANCE NO."
                        for(var tableRowIndex = 0; tableRowIndex < xlsData.Rows.Count; tableRowIndex++) {
                            for(var tableColumnIndex = 0;
                                tableColumnIndex < xlsData.Rows[tableRowIndex].ItemArray.Length;
                                tableColumnIndex++) {
                                if(xlsData.Rows[tableRowIndex].IsNull(tableColumnIndex) ||
                                    xlsData.Rows[tableRowIndex][tableColumnIndex].ToString() != "CLEARANCE NO.")
                                    continue;

                                matchTargetColumn = tableColumnIndex;
                                topLeftActiveRangeRow = tableRowIndex;
                                break;
                            }

                        }

                        if(topLeftActiveRangeRow == -1)
                            continue;

                        //Identify Active Range (Horizontal)
                        for(var tableColumnIndex = 0;
                            tableColumnIndex < xlsData.Rows[topLeftActiveRangeRow].ItemArray.Length;
                            tableColumnIndex++) {

                            if(topLeftActiveRangeColumn == -1 &&
                                !xlsData.Rows[topLeftActiveRangeRow].IsNull(tableColumnIndex))
                                topLeftActiveRangeColumn = tableColumnIndex;

                            if(topLeftActiveRangeColumn != -1 &&
                                !xlsData.Rows[topLeftActiveRangeRow].IsNull(tableColumnIndex) &&
                                xlsData.Rows[topLeftActiveRangeRow][tableColumnIndex].ToString() == "PO IDENTIFIER")
                                bottomRightActiveRangeColumn = tableColumnIndex;
                        }

                        if (topLeftActiveRangeColumn == -1 || bottomRightActiveRangeColumn == -1)
                        {
                            topLeftActiveRangeColumn     = -1;
                            topLeftActiveRangeRow        = -1;
                            bottomRightActiveRangeColumn = -1;
                            continue;
                        }

                        //Identify Active Range (Vertical)
                        xlsData.Clear();
                        xlsData.Dispose();
                        xlsData = new DataTable();
                        var characterTranslation = (char)(matchTargetColumn + 'A');
                        var query =
                            $"SELECT * FROM [{sheetName}${characterTranslation}:{characterTranslation}]";
                        var dbAdapterVertical = new OleDbDataAdapter(query, oleDbcon);
                        dbAdapterVertical.Fill(xlsData);
                        dbAdapterVertical.Dispose();

                        var bottomRightActiveRangeRow = xlsData.Rows.Count;
                        for(var tableRowIndex = topLeftActiveRangeRow;
                            tableRowIndex < xlsData.Rows.Count;
                            tableRowIndex++) {
                            if(!xlsData.Rows[tableRowIndex].IsNull(0))
                                continue;

                            bottomRightActiveRangeRow = tableRowIndex;
                            break;
                        }

                        if(bottomRightActiveRangeRow == -1) {
                            topLeftActiveRangeColumn = -1;
                            topLeftActiveRangeRow = -1;
                            bottomRightActiveRangeColumn = -1;
                            continue;
                        }

                        //Excel Sheet matches Blue Pass Clearance
                        //Proceed to Parse Document
                        var excelSheetActiveRange = $"{(char)(topLeftActiveRangeColumn + 'A')}{topLeftActiveRangeRow + 1}:{(char)(bottomRightActiveRangeColumn + 'A')}{bottomRightActiveRangeRow + 1}";

                        xlsData.Clear();
                        xlsData.Dispose();
                        xlsData = new DataTable();
                        query = $"SELECT * FROM [{sheetName}${excelSheetActiveRange}]";
                        var dbAdapterAllData = new OleDbDataAdapter(query, oleDbcon);
                        dbAdapterAllData.Fill(xlsData);
                        dbAdapterAllData.Dispose();

                        //Ensure Headers Match
                        if(xlsData.Rows[0][0].ToString().Trim().ToUpper() != "NRIC" ||
                            xlsData.Rows[0][1].ToString().Trim().ToUpper() != "RANK" ||
                            xlsData.Rows[0][2].ToString().Trim().ToUpper() != "NAME" ||
                            xlsData.Rows[0][3].ToString().Trim().ToUpper() != "COMPANY" ||
                            xlsData.Rows[0][4].ToString().Trim().ToUpper() != "AREA" ||
                            //Skip EXPIRY DATE may throw errors Column [H:H] Row 5
                            xlsData.Rows[0][6].ToString().Trim().ToUpper() != "SERIAL" ||
                            //Skip Color Column [H:H] Row 7
                            xlsData.Rows[0][8].ToString().Trim().ToUpper() != "STATUS" ||
                            xlsData.Rows[0][9].ToString().Trim().ToUpper() != "CLEARANCE NO." ||
                            xlsData.Rows[0][10].ToString().Trim().ToUpper() != "PO IDENTIFIER") {
                            topLeftActiveRangeColumn = -1;
                            topLeftActiveRangeRow = -1;
                            bottomRightActiveRangeColumn = -1;
                            continue;
                        }


                        for(var tableRowIndex = 1; tableRowIndex < xlsData.Rows.Count; tableRowIndex++) {
                            //Check Status and whether NRIC is valid
                            if(xlsData.Rows[tableRowIndex][8].ToString().ToUpper().Trim() != "VALID")
                                continue;

                            //NRIC
                            var nric = xlsData.Rows[tableRowIndex][0].ToString().Trim().ToUpper();
                            if(nric.Length != 9 || !nric.All(x => char.IsLetterOrDigit(x) || x == '-')) {
                                switch(ShowFormattingError("NRIC", "The NRIC should be 9 characters long.", topLeftActiveRangeRow + tableRowIndex, topLeftActiveRangeColumn, firstTimeShowingError, suppressErrorMessages)) {
                                    case MessageBoxResult.Cancel:
                                        throw new Exception($"Incorrect NRIC Format at row {topLeftActiveRangeRow + tableRowIndex}");

                                    case MessageBoxResult.Yes:
                                        firstTimeShowingError = true;
                                        suppressErrorMessages = true;
                                        break;

                                    case MessageBoxResult.No:
                                        firstTimeShowingError = true;
                                        break;

                                    default:
                                        break;
                                }
                                
                                if(nric.Length < 8) { }
                                if (nric.Length > 9)
                                    nric = nric.Substring(0, 9);
                                else if(nric.Length < 9)
                                    nric = nric.PadRight(9, '-');
                            }

                            //Area Access
                            var clearanceAreaAccess = Global.DataBase.FromStringToClearanceLevelEnum(xlsData.Rows[tableRowIndex][4].ToString().Trim().ToUpper());
                            if(clearanceAreaAccess == ClearanceLevelEnum.None) {
                                switch(ShowFormattingError("AREA", "Unrecognised Area", topLeftActiveRangeRow + tableRowIndex, topLeftActiveRangeColumn + 4, firstTimeShowingError, suppressErrorMessages)) {
                                    case MessageBoxResult.Cancel:
                                        throw new Exception($"Incorrect Unrecognised Area Format at row {topLeftActiveRangeRow + tableRowIndex}");

                                    case MessageBoxResult.Yes:
                                        firstTimeShowingError = true;
                                        suppressErrorMessages = true;
                                        break;

                                    case MessageBoxResult.No:
                                        firstTimeShowingError = true;
                                        break;

                                    default:
                                        break;
                                }
                            }

                            //DateTime
                            DateTime expiryDate;
                            if(xlsData.Rows[tableRowIndex][5] is DateTime time)
                                expiryDate = time;
                            else if(!DateTime.TryParseExact(xlsData.Rows[tableRowIndex][5].ToString().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out expiryDate)) {

                                switch(ShowFormattingError("EXPIRY DATE", "which should follow the format of \'dd/MM/yyyy\'.", topLeftActiveRangeRow + tableRowIndex, topLeftActiveRangeColumn + 5, firstTimeShowingError, suppressErrorMessages)) {
                                    case MessageBoxResult.Cancel:
                                        throw new Exception($"Incorrect EXPIRY DATE Format at row {topLeftActiveRangeRow + tableRowIndex}");

                                    case MessageBoxResult.Yes:
                                        firstTimeShowingError = true;
                                        suppressErrorMessages = true;
                                        break;

                                    case MessageBoxResult.No:
                                        firstTimeShowingError = true;
                                        break;

                                    default:
                                        break;
                                }
                            }

                            //Clearance
                            var clearanceDetails =
                                ((xlsData.Rows[tableRowIndex][10].ToString().Trim().Length > 0)
                                    ? $"{xlsData.Rows[tableRowIndex][10].ToString().ToUpper().Trim()} "
                                    : "") +
                                $"{xlsData.Rows[tableRowIndex][6].ToString().ToUpper().Trim()} (Clearance No: {xlsData.Rows[tableRowIndex][9].ToString().Trim().ToUpper()})";

                            //Salutations
                            var salutations = xlsData.Rows[tableRowIndex][1].ToString().Trim().ToUpper();
                            if(salutations.Length > 5)
                                salutations = salutations.Substring(0, 5);

                            //Full Name
                            var fullName = xlsData.Rows[tableRowIndex][2].ToString().Trim().ToUpper();
                            if(fullName.Length > 130)
                                fullName = fullName.Substring(0, 130);

                            //Company
                            var company = xlsData.Rows[tableRowIndex][3].ToString().Trim().ToUpper();
                            if(company.Length > 80)
                                company = company.Substring(0, 80);

                            resListofBluePassBulkClearances.Add(
                                new BluePassBulkClearance {
                                    NRIC = nric,
                                    AreaAccess = clearanceAreaAccess,
                                    EndDate = expiryDate,
                                    ClearanceDetails = clearanceDetails,
                                    Rank = salutations,
                                    FullName = fullName,
                                    Company = company
                                });
                        }
                    } catch (Exception) {
                        resListofBluePassBulkClearances.Clear();
                        topLeftActiveRangeColumn = -1;
                        topLeftActiveRangeRow = -1;
                        bottomRightActiveRangeColumn = -1;
                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(
                    "Please ensure the file is not used by another process\r\n\r\n" + ex.Message,
                    "Failed to open Excel file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                exceptionThrown = true;
            } finally {
                oleDbcon.Close();
                oleDbcon.Dispose();
            }

            if(!exceptionThrown && resListofBluePassBulkClearances.Count == 0) {
                MessageBox.Show(
                    "Please ensure that the Document is properly formatted and includes a table which matches the following column order:\r\n\r\n"
                    + "1)   NRIC\r\n2)   RANK\r\n3)   NAME\r\n4)   COMPANY\r\n5)   AREA\r\n6)   EXPIRY DATE\r\n7)   SERIAL\r\n"
                    + "8)   COLOUR\r\n9)   STATUS\r\n10) CLEARANCE NO.\r\n11) PO IDENTIFIER",
                    "Failed to Parse Excel file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            return resListofBluePassBulkClearances;
        }
        public MessageBoxResult ShowFormattingError(string formatErrorType, string formatsample, int row, int column, bool isFirstTime, bool suppressErrorMessages) {

            if(suppressErrorMessages)
                return MessageBoxResult.Yes;

            var dialogueResult = MessageBoxResult.No;

            if (!isFirstTime)
            {
                 dialogueResult = MessageBox.Show(
                    "Do you want to suppress further error messages telling you why the record is invalid?\r\n\r\nClick 'NO' to display every invalid record.",
                    "Parse Excel Document",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Exclamation);

                if (dialogueResult == MessageBoxResult.Cancel)
                    return MessageBoxResult.Cancel;

                if(dialogueResult == MessageBoxResult.Yes)
                    return MessageBoxResult.Yes;
            }

            return MessageBox.Show(
                       $"Irregular formatting of {formatErrorType} on Cell ${(char) (column + 'A')}{row + 1}\r\n"
                       + $"{formatsample}\r\n\r\n"
                       + "Do you wish to skip this clearance and carry on with parsing the excel sheet? "
                       + "This error will persist until the document is properly formatted.",
                       "Failed to Parse Excel file due to irregular format",
                       MessageBoxButton.YesNo,
                       MessageBoxImage.Error) == MessageBoxResult.No ? MessageBoxResult.Cancel : dialogueResult;
        }


        #region [Property Changed]

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChangedEvent(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
