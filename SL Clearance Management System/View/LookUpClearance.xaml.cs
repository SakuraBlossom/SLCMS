using System.Windows.Controls;

namespace SLCMS.View {
    /// <summary>
    /// Interaction logic for LookUpClearance.xaml
    /// </summary>
    public partial class LookUpClearance : UserControl {
        public LookUpClearance() {
            InitializeComponent();
        }

        private void VisitorInCampDataGrid_LoadingRow(object sender, DataGridRowEventArgs e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();

        private void VisitorInCampDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }
    }
}
