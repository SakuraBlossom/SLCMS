using System;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Input;
using SLCMS.BusinessLogic;

#pragma warning disable 0067

namespace SLCMS.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute) => _execute = execute;

        bool ICommand.CanExecute(object parameter) => true;

        [HandleProcessCorruptedStateExceptions]
        void ICommand.Execute(object parameter) {
            try { _execute(); }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An application error occurred. Please contact the adminstrator with the following information:\nPlease ensure the database is not used by another process\n\n{ex.Message}",
                    "Oooops... An unexpected error occurred! [Delegate]",
                    MessageBoxButton.OK);

                Global.MainViewModel.IsLoading = false;
            }
        }
    }
    //public class RelayCommand<T> : ICommand
    //{
    //    private readonly Action<T> _execute;
    //    private readonly Func<T, bool> _canExecute;

    //    event EventHandler ICommand.CanExecuteChanged
    //    {
    //        add => CommandManager.RequerySuggested += value;
    //        remove => CommandManager.RequerySuggested -= value;
    //    }

    //    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    //    {
    //        _execute = execute;
    //        _canExecute = canExecute;
    //    }

    //    bool ICommand.CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

    //    void ICommand.Execute(object parameter) => _execute((T)parameter);
    //}
}
