using System.Collections.Generic;
using Ferretto.VW.Utils.Source.CellsManagement;
using Ferretto.VW.Utils.Source.Configuration;

namespace Ferretto.VW.Utils.Source
{
    public static class DataManager
    {
        #region Properties

        public static List<Bay> Bays { get; set; }
        public static List<CellBlock> CellBlocks { get; set; }
        public static List<Cell> Cells { get; set; }
        public static List<Drawer> Drawers { get; set; }
        public static General_Info GeneralInfo { get; set; }
        public static Installation_Info InstallationInfo { get; set; }

        #endregion Properties
    }
}
