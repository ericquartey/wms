using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CellDetails : BaseModel<int>, ICellUpdatePolicy
    {
        #region Properties

        public string AbcClassId { get; set; }

        public int AisleId { get; set; }

        public int AreaId { get; set; }

        public int CellStatusId { get; set; }

        public int? CellTypeId { get; set; }

        [Positive]
        public int? Column { get; set; }

        [Positive]
        public int? Floor { get; set; }

        public int LoadingUnitsCount { get; set; }

        [Positive]
        public int? Number { get; set; }

        [Positive]
        public int Priority { get; set; }

        public Side Side { get; set; }

        [PositiveOrZero]
        public double? XCoordinate { get; set; }

        [PositiveOrZero]
        public double? YCoordinate { get; set; }

        [PositiveOrZero]
        public double? ZCoordinate { get; set; }

        #endregion
    }
}
