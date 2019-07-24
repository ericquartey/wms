namespace Ferretto.VW.Utils.Source.CellsManagement
{
    public sealed class Cell
    {
        #region Constructors

        public Cell(int id)
        {
            this.IdCell = id;
            this.Priority = id;
            if (id % 2 == 0)
            {
                this.Coord = (id == 2)
                    ? CellManagementMethods.CELL_HEIGHT_MILLIMETERS
                    : CellManagementMethods.CELL_HEIGHT_MILLIMETERS * (id / CellManagementMethods.AISLE_SIDES_COUNT);

                this.Side = CellSide.Front;
            }
            else
            {
                this.Coord = (id == 1)
                    ? CellManagementMethods.CELL_HEIGHT_MILLIMETERS
                    : CellManagementMethods.CELL_HEIGHT_MILLIMETERS * ((id / CellManagementMethods.AISLE_SIDES_COUNT) + 1);

                this.Side = CellSide.Back;
            }

            this.Status = CellStatus.Free;
        }

        #endregion

        #region Properties

        public int Coord { get; set; }

        public int IdCell { get; set; }

        public int Priority { get; set; }

        public CellSide Side { get; set; }

        public CellStatus Status { get; set; }

        #endregion
    }
}
