using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Corridoio
    public partial class Aisle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AreaId { get; set; }
        public int? Floors { get; set; }
        public int? Columns { get; set; }

        public Area Area { get; set; }

        public List<Cell> Cells { get; set; }
        public List<CellTypeAisle> AisleCellsTypes { get; set; }
        public List<LoadingUnitTypeAisle> AisleLoadingUnitTypes { get; set; }
        public List<CellTotal> CellTotals { get; set; }
        public List<CellsGroup> CellsGroups { get; set; }
    }
}
