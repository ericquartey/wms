using System.Collections.Generic;

namespace Ferretto.Common.Models
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

    public List<Cell> Cells { get; set; }
    public List<CellTypeAisle> CellTypeAisles { get; set; }
    public List<CellTotal> CellTotals { get; set; }
    public List<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }
  }
}
