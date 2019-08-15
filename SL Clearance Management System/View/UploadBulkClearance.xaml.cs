using System;
using System.Linq;
using System.Windows;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS.View {
    /// <summary>
    /// Interaction logic for UploadBulkClearance.xaml
    /// </summary>
    public partial class UploadBulkClearance {
        public readonly UploadBulkClearanceViewModel MyViewModel;

        public UploadBulkClearance() {
            InitializeComponent();
            DataContext = MyViewModel = new UploadBulkClearanceViewModel(BulkClearanceUploadTabControl);

            AreaDataGridComboBox.ItemsSource = Enum.GetValues(typeof(ClearanceLevelEnum)).Cast<ClearanceLevelEnum>().Skip(1);
        }
        private void Grid_DropFile(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                // Note that you can have more than one file.
                _ = MyViewModel.TryReadExcelSheet(((e.Data.GetData(DataFormats.FileDrop)) as string[])?[0]);
            }
        }





        private void OnClickToSelectionScreen(object sender, RoutedEventArgs e) {
            BulkClearanceUploadTabControl.SelectedIndex = 0;
        }
        private void OnClickToParseClearances(object sender, RoutedEventArgs e) {
            BulkClearanceUploadTabControl.SelectedIndex = 1;
        }

    }
}
