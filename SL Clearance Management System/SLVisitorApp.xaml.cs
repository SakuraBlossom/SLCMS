using System;
using System.IO;
using System.Reflection;
using System.Windows;
using SLCMS.BusinessLogic;

namespace SLCMS
{
    /// <summary>
    /// Interaction logic for SLVisitorApp.xaml
    /// </summary>
    public partial class SLVisitorApp {
        private void Application_Startup(object sender, StartupEventArgs e) {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += Application_DispatcherUnhandledException;

            StartupUri = new Uri("/SLCMS;component/SLVisitorManagementMainWindow.xaml", UriKind.Relative);
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs ex) {

            var errorFolder  = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var errorMessage = "Undefined Error - From CurrentDomain_UnhandledException()";
            try
            {
                if (ex.ExceptionObject is AccessViolationException accessViolationException)
                {
                    errorMessage = accessViolationException.Message;
                    File.WriteAllText(errorFolder + "//error.txt", accessViolationException.ToString());
                } else if (ex.ExceptionObject is Exception exception)
                {
                    errorMessage = exception.Message;
                    File.WriteAllText(errorFolder + "//error.txt", exception.ToString());
                }
            } catch (Exception) { }

            MessageBox.Show(
                $"An application error occurred. Please contact the adminstrator with the following information:\n\n{errorMessage}",
                "Oooops... An unexpected error occurred! [Domain]",
                MessageBoxButton.OK);
        }

        public static void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs ex) {
            try
            {
                var errorFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                File.WriteAllText(errorFolder + "//error.txt", ex.Exception.ToString());
            } catch (Exception) { }

            MessageBox.Show(
                $"An application error occurred. Please contact the adminstrator with the following information:\n\n{ex.Exception.Message}\n\nYou are encouraged to restart this program after completing your action.",
                "Oooops... An unexpected error occurred! [Dispatcher]",
                MessageBoxButton.OK);

            ex.Handled = true;
            Global.MainViewModel.IsLoading = false;
        }

    }
}
