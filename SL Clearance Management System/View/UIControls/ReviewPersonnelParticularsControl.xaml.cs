using System.Windows;
using System.Windows.Controls;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.View.UIControls
{
    /// <summary>
    /// Interaction logic for ReviewPersonnelParticularsControl.xaml
    /// </summary>
    public partial class ReviewPersonnelParticularsControl {
        public ReviewPersonnelParticularsControl()
        {
            InitializeComponent();
        }

        private void LookupButton_Click(object sender, RoutedEventArgs e) {
            if (DataContext is PersonnelDetails details)
            {
                Global.MainViewModel.ViewModelVisitorHistoryLookup.LookupVisitorHistory(
                    details.NRIC,
                    details.RankAndName);
            }
        }
    }
}
