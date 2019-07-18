using Ferretto.VW.MAS.DataLayer.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Models
{
    public class Cell
    {
        #region Properties

        public int CellId { get; set; }

        public decimal Coord { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int LoadingUnitId { get; set; }

        public int Priority { get; set; }

        public Side Side { get; set; }

        public Status Status { get; set; }

        public Status WorkingStatus { get; set; }

        #endregion
    }
}
