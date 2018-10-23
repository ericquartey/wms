using System.Windows;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private InstallationApp.MainWindowViewModel installationAppMainWindowViewModel = new InstallationApp.MainWindowViewModel();

        #endregion Fields

        #region Constructors

        public App()
        {
            this.InstallationMainWindowInstance.DataContext = this.installationAppMainWindowViewModel;
        }

        #endregion Constructors

        #region Properties

        public InstallationApp.MainWindow InstallationMainWindowInstance { get; set; } = new InstallationApp.MainWindow();
        public InstallationApp.Window1 InstallationWindow1Instance { get; set; }
        public InstallationApp.Window2 InstallationWindow2Instance { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties
    }
}
