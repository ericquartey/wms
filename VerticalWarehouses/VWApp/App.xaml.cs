using System;
using System.Configuration;
using System.Windows;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;
using Newtonsoft.Json;
using System.IO;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private static readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);
        private static readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);

        private bool machineOk;

        #endregion Fields

        #region Constructors

        public App()
        {
            this.InitializeComponent();
            NavigationService.InitializeEvents();
            DataMngr.CurrentData = new DataMngr();
        }

        #endregion Constructors

        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }

        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }

        public Boolean MachineOk { get => this.machineOk; set => this.machineOk = value; }

        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties
    }
}
