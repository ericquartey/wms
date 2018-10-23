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

        public InstallationApp.MainWindow InstallationMainWindowInstance { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties
    }
}
