using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class AvailableCell
    {
        #region Constructors

        public AvailableCell(Cell cell, double height, bool isFloating)
        {
            this.Cell = cell;
            this.Height = height;
            this.IsFloating = isFloating;
        }

        #endregion

        #region Properties

        public Cell Cell { get; set; }

        public double Height { get; set; }

        public bool IsFloating { get; set; }

        #endregion
    }
}
