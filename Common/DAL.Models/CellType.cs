using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
    // Tipo Cella
    public sealed class CellType
    {
        public int Id { get; set; }
        public int CellHeightClassId { get; set; }
        public int CellWeightClassId { get; set; }
        public int CellSizeClassId { get; set; }
        public string Description { get; set; }

        public CellHeightClass CellHeightClass { get; set; }
        public CellWeightClass CellWeightClass { get; set; }
        public CellSizeClass CellSizeClass { get; set; }

        public IEnumerable<Cell> Cells { get; set; }
        public IEnumerable<CellTypeAisle> CellTypeAisles { get; set; }
        public IEnumerable<CellTotal> CellTotals { get; set; }
        public IEnumerable<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }
    }
}
