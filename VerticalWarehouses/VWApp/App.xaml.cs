using System.Windows;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {//Instances of differents' projects MainWindow are declared in App.xaml.cs, so that there will be no running processes after closing VWApp.
        #region Properties

        public InstallationApp.MainWindow InstallationMainWindowInstance { get; set; }
        public InstallationApp.Window1 InstallationWindow1Instance { get; set; }
        public InstallationApp.Window2 InstallationWindow2Instance { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties
    }
}
