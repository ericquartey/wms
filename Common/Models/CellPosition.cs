using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Posizione in Cella
  public sealed class CellPosition
  {
    public int Id { get; set; }
    public int? XOffset { get; set; }
    public int? YOffset { get; set; }
    public int? ZOffset { get; set; }
    public string Description { get; set; }

    public IEnumerable<LoadingUnit> LoadingUnits { get; set; }
    public IEnumerable<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes { get; set; }
  }
}
