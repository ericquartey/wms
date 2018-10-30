using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source.CellsManagement
{
    public class Cell
    {
        #region Constructors

        public Cell(int id)
        {
            this.IdCell = id;
            this.Priority = id;
            if (id % 2 == 0) //if id is even
            {
                this.Coord = (id == 2) ? CellManagementMethods.CELL_HEIGHT_MILLIMETERS : CellManagementMethods.CELL_HEIGHT_MILLIMETERS * (id / CellManagementMethods.AISLE_SIDES_COUNT);
                this.Side = Side.FrontEven;
            }
            else //if id is odd
            {
                this.Coord = (id == 1) ? CellManagementMethods.CELL_HEIGHT_MILLIMETERS : CellManagementMethods.CELL_HEIGHT_MILLIMETERS * ((id / CellManagementMethods.AISLE_SIDES_COUNT) + 1);
                this.Side = Side.BackOdd;
            }
            this.Status = Status.Free;
        }

        #endregion Constructors

        #region Properties

        public Int32 Coord { get; set; }
        public Int32 IdCell { get; set; }
        public Int32 Priority { get; set; }
        public Side Side { get; set; }
        public Status Status { get; set; }

        #endregion Properties

        // status code: 0 = free; 1 = disabled; 2 = occupied; 3 = unusable
    }
}
