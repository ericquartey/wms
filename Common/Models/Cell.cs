using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Cella
    public partial class Cell
    {
        public int Id { get; set; }
        public int AisleId { get; set; }
        public int? Floor { get; set; }
        public int? Column { get; set; }
        public Side Side { get; set; }
        public int? CellNumber { get; set; }
        public int? XCoordinate { get; set; }
        public int? YCoordinate { get; set; }
        public int? ZCoordinate { get; set; }
        public int Priority { get; set; }
        public int? CellTypeId { get; set; }
        public AbcClass Class { get; set; }
        public int CellStatusId { get; set; }

        public Aisle Aisle { get; set; }
        public CellStatus CellStatus { get; set; }
        public CellType CellType { get; set; }

        public List<LoadingUnit> LoadingUnits { get; set; }
        public List<CellsGroup> FirstCellsGroups { get; set; }
        public List<CellsGroup> LastCellsGroups { get; set; }
    }
}
