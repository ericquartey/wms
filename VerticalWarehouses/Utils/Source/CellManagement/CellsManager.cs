using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source.CellManagement
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

        public Int32 BayCounter { get; set; }
        public List<Bay> Bays { get; set; }
        public List<CellBlock> Blocks { get; set; }
        public List<Cell> Cells { get; set; }
        public List<Drawer> Drawers { get; set; }

        #endregion Properties
    }
}
