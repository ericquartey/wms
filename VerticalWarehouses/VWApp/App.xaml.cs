using System.Diagnostics;
using System.Windows;
using Ferretto.VW.InstallationApp;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Constructors

        public App()
        {
            this.InitializeComponent();
            this.InstallationAppMainWindowInstance = new InstallationApp.MainWindow();
            this.InstallationAppMainWindowInstance.InitializeComponent();
            this.InstallationAppMainWindowInstance.InitializeNavigation();
            this.InstallationAppMainWindowViewModel = new InstallationApp.MainWindowViewModel();
            this.InstallationAppMainWindowInstance.DataContext = this.InstallationAppMainWindowViewModel;
        }

        #endregion Constructors

        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }
        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties

        #region Methods

        private void InitializeData()
        {
        }

        #endregion Methods
    }
}
