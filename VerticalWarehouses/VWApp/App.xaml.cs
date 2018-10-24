using System;
using System.Configuration;
using System.Windows;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {
        #region Fields

        private static readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);
        private static readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);

        #endregion Fields

        #region Properties

        public CellsManager AppCellsManager { get; set; }
        public General_Info GeneralInfo { get; set; }
        public InstallationApp.MainWindow InstallationAppMainWindowInstance { get; set; }
        public InstallationApp.MainWindowViewModel InstallationAppMainWindowViewModel { get; set; }
        public Installation_Info InstallationInfo { get; set; }
        public OperatorApp.MainWindow OperatorMainWindowInstance { get; set; }

        #endregion Properties

        #region Methods

        private void InitializeData()
        {
        }

        private void ReadDataFromFile()
        {
            var json0 = File.ReadAllText(JSON_GENERAL_INFO_PATH);
            JsonConvert.DeserializeAnonymousType(json0, this.GeneralInfo);
            var json1 = File.ReadAllText(JSON_INSTALLATION_INFO_PATH);
            JsonConvert.DeserializeAnonymousType(json1, this.InstallationInfo);
        }

        #endregion Methods
    }
}
