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
        private static Installation_Info installationInfo;

        #endregion Fields

        #region Properties

        public static List<CellBlock> CellBlocks { get => cellBlocks; set { cellBlocks = value; RaiseCellBlocksChangedEvent(); } }
        public static List<Cell> Cells { get => cells; set { cells = value; RaiseCellsChangedEvent(); } }
        public static List<Drawer> Drawers { get => drawers; set { drawers = value; RaiseDrawersChangedEvent(); } }
        public static General_Info GeneralInfo { get; set; }
        public static Installation_Info InstallationInfo { get => installationInfo; set { installationInfo = value; RaiseInstallationInfoChangedEvent(); } }

        #endregion Properties

        #region Methods

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
            UpdateInstallationInfoFile();
            NavigationService.RaiseInstallationInfoChangedEvent();
        }

        private static void UpdateInstallationInfoFile()
        {
            var json = JsonConvert.SerializeObject(InstallationInfo, Formatting.Indented);
            if (File.Exists(JSON_INSTALLATION_INFO_PATH))
            {
                File.Delete(JSON_INSTALLATION_INFO_PATH);
                File.WriteAllText(JSON_INSTALLATION_INFO_PATH, json);
            }
            else
            {
                File.WriteAllText(JSON_INSTALLATION_INFO_PATH, json);
            }
        }

        #endregion Methods
    }
}
