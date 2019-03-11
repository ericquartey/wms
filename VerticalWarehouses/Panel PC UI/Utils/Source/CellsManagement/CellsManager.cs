using System;
using System.Collections.Generic;

namespace Ferretto.VW.Utils.Source.CellsManagement
{
    public class CellsManager
    {
        #region Constructors

        public CellsManager()
        {
            this.Bays = new List<Bay>();
            this.Blocks = new List<CellBlock>();
            this.Cells = new List<Cell>();
            this.Drawers = new List<Drawer>();
        }

        #endregion Constructors

        #region Properties

        public int BayCounter { get; set; }

        public List<Bay> Bays { get; set; }
        public List<CellBlock> Blocks { get; set; }
        public List<Cell> Cells { get; set; }
        public List<Drawer> Drawers { get; set; }

        #endregion Properties
    }
}
