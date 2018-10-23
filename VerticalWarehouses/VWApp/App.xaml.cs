using System.Windows;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Properties

        public InstallationApp.MainWindow InstallationMainWindowInstance { get; set; }
        public InstallationApp.Window1 InstallationWindow1Instance { get; set; }
        public InstallationApp.Window2 InstallationWindow2Instance { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties
    }
}
