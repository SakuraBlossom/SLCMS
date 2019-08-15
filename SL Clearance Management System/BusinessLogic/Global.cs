using System;
using System.Windows;
using SLCMS.ViewModel;

namespace SLCMS.BusinessLogic
{
    public static class Global
    {
        public static SLDataBaseConnection DataBase;
        public static MainWindowViewModel MainViewModel;
        public static SLVisitorManagementMainWindow SLVisitorManagementMainWindow;
        public static void ForceGarbageCollector() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
