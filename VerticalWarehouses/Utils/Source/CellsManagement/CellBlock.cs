using System;

namespace Ferretto.VW.Utils.Source.CellsManagement
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

        public int Area { get; set; }

        public int BlockHeightMillimiters
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

        public int FinalIDCell { get; set; }
        public int IdGroup { get; set; }
        public int InitialIDCell { get; set; }
        public int Machine { get; set; }
        public int Priority { get; set; }
        public Side Side { get; set; }

        #endregion Properties
    }
}
