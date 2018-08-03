using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Corridoio
  public sealed class Aisle
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int AreaId { get; set; }
    public int? Floors { get; set; }
    public int? Columns { get; set; }

    public Area Area { get; set; }

    public IEnumerable<Cell> Cells { get; set; }
    public IEnumerable<CellTypeAisle> AisleCellsTypes { get; set; }
    public IEnumerable<LoadingUnitTypeAisle> AisleLoadingUnitTypes { get; set; }
    public IEnumerable<CellTotal> CellTotals { get; set; }
    public IEnumerable<CellsGroup> CellsGroups { get; set; }
    public IEnumerable<Machine> Machines { get; set; }
  }
}
