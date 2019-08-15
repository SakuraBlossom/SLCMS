using System.Windows.Controls;
using SLCMS.ViewModel;

namespace SLCMS.View {
    /// <summary>
    /// Interaction logic for VisitorHistoryLookup.xaml
    /// </summary>
    public partial class VisitorHistoryLookup {
        public VisitorHistoryLookupViewModel MyViewModel;

        public VisitorHistoryLookup() {
            InitializeComponent();
            DataContext = MyViewModel = new VisitorHistoryLookupViewModel(CloseDialogButton);
        }

        private void VisitorRecordsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            CloseDialogButton.Command?.Execute(null);
        }
    }
}
