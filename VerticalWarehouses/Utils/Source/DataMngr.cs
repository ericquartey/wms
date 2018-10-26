using System.Collections.Generic;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;
using Ferretto.VW.Navigation;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Configuration;
using System.Diagnostics;

namespace Ferretto.VW.Utils.Source
{
    public class DataMngr
    {
        #region Fields

        public static DataMngr CurrentData;

        private readonly string JSON_GENERAL_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["GeneralInfoFilePath"]);
        private readonly string JSON_INSTALLATION_INFO_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["InstallationInfoFilePath"]);

        private List<CellBlock> cellBlocks;
        private List<Cell> cells;
        private List<Drawer> drawers;
        private Installation_Info installationInfo;

        #endregion Fields

        #region Constructors

        public DataMngr()
        {
            this.InitializeDataManager();
        }

        #endregion Constructors

        #region Properties

        public List<CellBlock> CellBlocks { get => this.cellBlocks; set { this.cellBlocks = value; this.RaiseCellBlocksChangedEvent(); } }
        public List<Cell> Cells { get => this.cells; set { this.cells = value; this.RaiseCellsChangedEvent(); } }
        public List<Drawer> Drawers { get => this.drawers; set { this.drawers = value; this.RaiseDrawersChangedEvent(); } }
        public General_Info GeneralInfo { get; set; }
        public Installation_Info InstallationInfo { get => this.installationInfo; set { this.installationInfo = value; this.RaiseInstallationInfoChangedEvent(); } }

        #endregion Properties

        #region Methods

        public void InitializeDataManager()
        {
            if (File.Exists(this.JSON_GENERAL_INFO_PATH) && File.Exists(this.JSON_INSTALLATION_INFO_PATH))
            {
                var jsonGI = File.ReadAllText(this.JSON_GENERAL_INFO_PATH);
                var jsonII = File.ReadAllText(this.JSON_INSTALLATION_INFO_PATH);
                this.GeneralInfo = JsonConvert.DeserializeObject<General_Info>(jsonGI);
                this.InstallationInfo = JsonConvert.DeserializeObject<Installation_Info>(jsonII);
            }
        }

        public void UpdateInstallationInfoFile()
        {
            var json = JsonConvert.SerializeObject(this.InstallationInfo, Formatting.Indented);
            File.WriteAllText(this.JSON_INSTALLATION_INFO_PATH, json);
        }

        private void CreateDummyFiles()
        {
            var gi = new General_Info(0);
            var ii = new Installation_Info(0);

            var jsonGI = JsonConvert.SerializeObject(gi, Formatting.Indented);
            var jsonii = JsonConvert.SerializeObject(ii, Formatting.Indented);

            File.WriteAllText(this.JSON_GENERAL_INFO_PATH, jsonGI);
            File.WriteAllText(this.JSON_INSTALLATION_INFO_PATH, jsonii);
        }

        private void RaiseCellBlocksChangedEvent()
        {
            NavigationService.RaiseCellBlockChangedEvent();
        }

        private void RaiseCellsChangedEvent()
        {
            NavigationService.RaiseCellsChangedEvent();
        }

        private void RaiseDrawersChangedEvent()
        {
            NavigationService.RaiseDrawersChangedEvent();
        }

        private void RaiseInstallationInfoChangedEvent()
        {
            this.UpdateInstallationInfoFile();
            NavigationService.RaiseInstallationInfoChangedEvent();
        }

        #endregion Methods
    }
}
