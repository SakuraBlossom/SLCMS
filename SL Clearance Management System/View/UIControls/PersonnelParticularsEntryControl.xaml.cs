using System.Windows;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;

namespace SLCMS.View.UIControls
{
    /// <summary>
    /// Interaction logic for PersonnelParticularsEntryControl.xaml
    /// </summary>
    public partial class PersonnelParticularsEntryControl {
        public static readonly DependencyProperty IsPersonnelParticularsReadOnlyProperty
            = DependencyProperty.Register(
                "IsPersonnelParticularsReadOnly",
                typeof(bool),
                typeof(PersonnelParticularsEntryControl),
                new PropertyMetadata(false)
            );
        public bool IsPersonnelParticularsReadOnly
        {
            get => (bool)GetValue(IsPersonnelParticularsReadOnlyProperty);
            set
            {
                SetValue(IsPersonnelParticularsReadOnlyProperty, value);
                RankSelectionComboBox.IsEnabled = !value;
            }
        }

        public static readonly DependencyProperty EnableRemoveSelectFeatureProperty
            = DependencyProperty.Register(
                "EnableRemoveSelectFeature",
                typeof(bool),
                typeof(PersonnelParticularsEntryControl),
                new PropertyMetadata(true)
            );
        public bool EnableRemoveSelectFeature
        {
            get => (bool)GetValue(EnableRemoveSelectFeatureProperty);
            set => SetValue(EnableRemoveSelectFeatureProperty, value);
        }

        public static readonly DependencyProperty ButtonCommandProperty
            = DependencyProperty.Register(
                "ButtonCommand",
                typeof(ICommand),
                typeof(PersonnelParticularsEntryControl),
                null
            );
        public ICommand ButtonCommand
        {
            get => (ICommand)GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }

        public PersonnelParticularsEntryControl()
        {
            InitializeComponent();
        }
        private void LookupButton_Click(object sender, RoutedEventArgs e) {
            if(DataContext is PersonnelDetails details) {
                Global.MainViewModel.ViewModelVisitorHistoryLookup.LookupVisitorHistory(
                    details.NRIC,
                    details.RankAndName);
            }
        }
    }
}
