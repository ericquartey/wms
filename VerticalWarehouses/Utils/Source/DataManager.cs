using System.Collections.Generic;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;
using Ferretto.VW.Navigation;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Configuration;

namespace Ferretto.VW.Utils.Source
{
    public static class DataManager
    {
        #region Fields

        private static readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);
        private static readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);

        private static List<CellBlock> cellBlocks;
        private static List<Cell> cells;
        private static List<Drawer> drawers;
        private static Installation_Info installationInfo = new Installation_Info();

        #endregion Fields

        #region Properties

        public static List<CellBlock> CellBlocks { get => cellBlocks; set { cellBlocks = value; RaiseCellBlocksChangedEvent(); } }
        public static List<Cell> Cells { get => cells; set { cells = value; RaiseCellsChangedEvent(); } }
        public static List<Drawer> Drawers { get => drawers; set { drawers = value; RaiseDrawersChangedEvent(); } }
        public static General_Info GeneralInfo { get; set; } = new General_Info();
        public static Installation_Info InstallationInfo { get => installationInfo; set { installationInfo = value; RaiseInstallationInfoChangedEvent(); } }

        #endregion Properties

        #region Methods

        public static void InitializeDataManager()
        {
            if (File.Exists(JSON_GENERAL_INFO_PATH) && File.Exists(JSON_INSTALLATION_INFO_PATH))
            {
                var _InstallationInfo = new Installation_Info();
                var _GeneralInfo = new General_Info();
                var json0 = File.ReadAllText(JSON_GENERAL_INFO_PATH);
                JsonConvert.DeserializeAnonymousType(json0, _GeneralInfo);
                var json1 = File.ReadAllText(JSON_INSTALLATION_INFO_PATH);
                JsonConvert.DeserializeAnonymousType(json1, _InstallationInfo);

                GeneralInfo = _GeneralInfo;
                InstallationInfo = _InstallationInfo;
            }
        }

        public static void UpdateInstallationInfoFile()
        {
            var json = JsonConvert.SerializeObject(InstallationInfo, Formatting.Indented);
            File.WriteAllText(JSON_INSTALLATION_INFO_PATH, json);
        }

        private static void RaiseCellBlocksChangedEvent()
        {
            NavigationService.RaiseCellBlockChangedEvent();
        }

        private static void RaiseCellsChangedEvent()
        {
            NavigationService.RaiseCellsChangedEvent();
        }

        private static void RaiseDrawersChangedEvent()
        {
            NavigationService.RaiseDrawersChangedEvent();
        }

        private static void RaiseInstallationInfoChangedEvent()
        {
            NavigationService.RaiseInstallationInfoChangedEvent();
        }

        #endregion Methods
    }
}
