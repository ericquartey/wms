using System.Collections.Generic;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.Common.DataModels
{
    // Cella
    public sealed class Cell : IDataModel<int>
    {
        #region Properties

        public AbcClass AbcClass { get; set; }

        public string AbcClassId { get; set; }

        public Aisle Aisle { get; set; }

        public int AisleId { get; set; }

        public int? CellNumber { get; set; }

        public CellStatus CellStatus { get; set; }

        public int CellStatusId { get; set; }

        public CellType CellType { get; set; }

        public int? CellTypeId { get; set; }

        public int? Column { get; set; }

        public int? Floor { get; set; }

        public int Id { get; set; }

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }

        public IEnumerable<Mission> Missions { get; set; }

        public int Priority { get; set; }

        public Enums.Side Side { get; set; }

        public double? XCoordinate { get; set; }

        public double? YCoordinate { get; set; }

        public double? ZCoordinate { get; set; }

        #endregion
    }
}
