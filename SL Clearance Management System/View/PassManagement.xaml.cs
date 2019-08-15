using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SLCMS.BusinessLogic;
using SLCMS.Model;
using SLCMS.ViewModel;

namespace SLCMS.View
{
    /// <summary>
    /// Interaction logic for PassManagement.xaml
    /// </summary>
    public partial class PassManagement {
        public PassManagementViewModel MyViewModel;

        public PassManagement()
        {
            InitializeComponent();
            DataContext = MyViewModel = new PassManagementViewModel();

            AreaDataGridComboBox.ItemsSource = Enum.GetValues(typeof(ClearanceLevelEnum)).Cast<ClearanceLevelEnum>().Skip(1).Take(4);
            StatusDataGridComboBox.ItemsSource = Enum.GetValues(typeof(PassConditionEnum)).Cast<PassConditionEnum>();
        }

        private void DeletePassButton_Click(object sender, RoutedEventArgs e) {
            SearchTextBox.Clear();
            var model = (sender as Button)?.DataContext as VisitorPass;
            MyViewModel.RemoveVisitorPass(model);
            VisitorPassDataGrid.Focus();
        }
        private void LookupPassButton_Click(object sender, RoutedEventArgs e) {
            if ((sender as Button)?.DataContext is VisitorPass model)
                Global.MainViewModel.ViewModelVisitorHistoryLookup.LookupVisitorPassHistory(model.PassId);
        }

        private void MoveUpPassButton_Click(object sender, RoutedEventArgs e) {
            SearchTextBox.Clear();
            var model = (sender as Button)?.DataContext as VisitorPass;
            MyViewModel.MoveUp(model);
            VisitorPassDataGrid.SelectedItem = model;
            VisitorPassDataGrid.Focus();
        }
        private void MoveDownPassButton_Click(object sender, RoutedEventArgs e) {
            SearchTextBox.Clear();
            var model = (sender as Button)?.DataContext as VisitorPass;
            MyViewModel.MoveDown(model);
            VisitorPassDataGrid.SelectedItem = model;
            VisitorPassDataGrid.Focus();
        }

        private void NewEntryButton_Click(object sender, RoutedEventArgs e) {
            var newEntryName = NewEntryVisitorPassTextBox?.Text.ToUpper();
            if(newEntryName != null && newEntryName.Length <= 3)
                return;

            var newEntry = MyViewModel.ListofVisitorPasses.FirstOrDefault(x => x.PassId.ToUpper() == newEntryName);
            if (newEntry != null) {
                VisitorPassDataGrid.SelectedItem = newEntry;
                VisitorPassDataGrid.ScrollIntoView(newEntry);
                return;
            }

            newEntry = new VisitorPass {
                PassId            = newEntryName,
                AreaAccess        = ClearanceLevelEnum.A,
                PassCondition     = PassConditionEnum.Available,
                RequireEscort     = false,
                OrderOfPerference = MyViewModel.ListofVisitorPasses.Count + 1,
                IsDirty           = true
            };

            MyViewModel.ListofVisitorPasses.Add(newEntry);
            VisitorPassDataGrid.SelectedItem = newEntry;
            VisitorPassDataGrid.ScrollIntoView(newEntry);
        }
        
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var textToMatch = SearchTextBox?.Text;
            var dataGridCollectionViewSource = CollectionViewSource.GetDefaultView(VisitorPassDataGrid.ItemsSource);

            if(string.IsNullOrEmpty(textToMatch) || textToMatch.Length < 2 || !((VisitorPassDataGrid.ItemsSource) is IList<VisitorPass>)) {
                
                dataGridCollectionViewSource.Filter = null;
                return;
            }

            dataGridCollectionViewSource.Filter = arg => (arg is VisitorPass x) && (x.PassId.Contains(textToMatch) || x.AreaAccessString.Contains(textToMatch));
        }

        private void VisitorPassDataGrid_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key)
            {
                case Key.Delete: {
                    e.Handled = true;
                    var listofSelectedPasses = VisitorPassDataGrid.SelectedItems.Cast<VisitorPass>().OrderBy(x => x.OrderOfPerference).ToArray();
                    for(var count = 0; count < listofSelectedPasses.Length; count++) {
                        MyViewModel.RemoveVisitorPass(listofSelectedPasses[count]);
                    }
                    break;
                }


                case Key.Add:
                case Key.OemPlus: {
                    SearchTextBox.Clear();
                    e.Handled = true;
                    var listofSelectedPasses = VisitorPassDataGrid.SelectedItems.Cast<VisitorPass>().OrderBy(x => x.OrderOfPerference).ToArray();
                    if(listofSelectedPasses == null || listofSelectedPasses[0].OrderOfPerference <= 1)
                        break;
                    VisitorPassDataGrid.SelectedItems.Clear();
                    for (var count = 0; count < listofSelectedPasses.Length; count++) {
                        MyViewModel.MoveUp(listofSelectedPasses[count]);
                        VisitorPassDataGrid.SelectedItems.Add(listofSelectedPasses[count]);
                    }
                    VisitorPassDataGrid.ScrollIntoView(listofSelectedPasses[0]);
                    VisitorPassDataGrid.Focus();
                    break;
                }
                case Key.Subtract:
                case Key.OemMinus: {
                    SearchTextBox.Clear();
                    e.Handled = true;
                    var listofSelectedPasses = VisitorPassDataGrid.SelectedItems.Cast<VisitorPass>().OrderBy(x => x.OrderOfPerference).ToArray();
                    if(listofSelectedPasses == null || listofSelectedPasses[listofSelectedPasses.Length - 1].OrderOfPerference >= MyViewModel.ListofVisitorPasses.Count)
                        break;
                    VisitorPassDataGrid.SelectedItems.Clear();
                    for (var count = listofSelectedPasses.Length - 1; count >= 0; count--) {
                        MyViewModel.MoveDown(listofSelectedPasses[count]);
                        VisitorPassDataGrid.SelectedItems.Add(listofSelectedPasses[count]);
                    }
                    VisitorPassDataGrid.ScrollIntoView(listofSelectedPasses[listofSelectedPasses.Length - 1]);
                    VisitorPassDataGrid.Focus();
                    break;
                    }
            }
        }

    }
}
