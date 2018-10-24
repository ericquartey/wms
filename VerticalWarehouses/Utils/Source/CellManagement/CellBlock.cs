using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source.CellManagement
{
    public class CellBlock
    {
        #region Fields

        private int blockHeightMillimiters;

        #endregion Fields

        #region Constructors

        public CellBlock(int firstCellID, int lastCellID, int blockID)
        {
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            this.InitialIDCell = firstCellID;
            this.FinalIDCell = lastCellID;
            this.Priority = this.InitialIDCell;
            this.BlockHeightMillimiters = ((lastCellID - firstCellID) / 2) * CellManagementMethods.CELL_HEIGHT_MILLIMETERS;
            this.Side = (firstCellID % 2 == 0) ? Side.FrontEven : Side.BackOdd;
            this.IdGroup = blockID;
        }

        #endregion Constructors

        #region Properties

        public Int32 Area { get; set; }

        public Int32 BlockHeightMillimiters
        {
            get => this.blockHeightMillimiters;
            set
            {
                if (value >= 0)
                {
                    this.blockHeightMillimiters = value;
                }
                else
                {
                    this.blockHeightMillimiters = 0;
                }
            }
        }

        public Int32 FinalIDCell { get; set; }
        public Int32 IdGroup { get; set; }
        public Int32 InitialIDCell { get; set; }
        public Int32 Machine { get; set; }
        public Int32 Priority { get; set; }
        public Side Side { get; set; }

        #endregion Properties
    }
}
