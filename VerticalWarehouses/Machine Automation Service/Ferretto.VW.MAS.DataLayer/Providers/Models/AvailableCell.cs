using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class AvailableCell
    {
        #region Constructors

        public AvailableCell(Cell cell, double height)
        {
            this.Cell = cell;
            this.Height = height;
        }

        #endregion

        #region Properties

        public Cell Cell { get; set; }

        public double Height { get; set; }

        #endregion
    }
}
