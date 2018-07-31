using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Posizione in Cella
  public partial class CellPosition
  {
    public int Id { get; set; }
    public int? XOffset { get; set; }
    public int? YOffset { get; set; }
    public int? ZOffset { get; set; }
    public string Description { get; set; }

    public List<LoadingUnit> LoadingUnits { get; set; }
    public List<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes { get; set; }
  }
}
