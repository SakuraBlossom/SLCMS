using System;
using System.Windows;

namespace SLCMS {
    /// <summary>
    /// Interaction logic for SLClearanceApp.xaml
    /// </summary>
    public partial class SLClearanceApp {
        private void Application_Startup(object sender, StartupEventArgs e) {
            AppDomain.CurrentDomain.UnhandledException += SLVisitorApp.CurrentDomain_UnhandledException;
            DispatcherUnhandledException += SLVisitorApp.Application_DispatcherUnhandledException;

            StartupUri = new Uri("/SLCMS;component/SLVisitorManagementMainWindow.xaml", UriKind.Relative);
        }
    }
}
