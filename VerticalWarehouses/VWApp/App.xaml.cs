using System;
using System.Configuration;
using System.Windows;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;
using Newtonsoft.Json;
using System.IO;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private static readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);
        private static readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);

        #endregion Fields

        #region Constructors

        public App()
        {
            this.InitializeComponent();
            this.ReadDataFromFile();
        }

        #endregion Constructors

        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }
        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties

        #region Methods

        private void ReadDataFromFile()
        {
            var InstallationInfo = new Installation_Info();
            var GeneralInfo = new General_Info();
            var json0 = File.ReadAllText(JSON_GENERAL_INFO_PATH);
            JsonConvert.DeserializeAnonymousType(json0, GeneralInfo);
            var json1 = File.ReadAllText(JSON_INSTALLATION_INFO_PATH);
            JsonConvert.DeserializeAnonymousType(json1, InstallationInfo);

            DataManager.GeneralInfo = GeneralInfo;
            DataManager.InstallationInfo = InstallationInfo;
        }

        #endregion Methods
    }
}
