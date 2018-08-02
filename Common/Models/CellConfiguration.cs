using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Configurazione Cella
  public sealed class CellConfiguration
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public List<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes { get; set; }
    public List<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }
  }
}
