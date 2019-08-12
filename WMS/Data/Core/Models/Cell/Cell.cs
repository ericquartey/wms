using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Cell))]
    public class Cell : BaseModel<int>, ICellUpdatePolicy
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public int AbcClassId { get; set; }

        public int AisleId { get; set; }

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        public string CellTypeDescription { get; set; }

        [Positive]
        public int? Column { get; set; }

        [Positive]
        public int? Floor { get; set; }

        public int LoadingUnitsCount { get; set; }

        public string LoadingUnitsDescription { get; set; }

        [Positive]
        public int? Number { get; set; }

        [Positive]
        public int Priority { get; set; }

        public Enums.Side Side { get; set; }

        public string Status { get; set; }

        [PositiveOrZero]
        public double? XCoordinate { get; set; }

        [PositiveOrZero]
        public double? YCoordinate { get; set; }

        [PositiveOrZero]
        public double? ZCoordinate { get; set; }

        #endregion
    }
}
