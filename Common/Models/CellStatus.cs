using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Stato Cella
  public sealed class CellStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public List<Cell> Cells { get; set; }
    public List<CellTotal> CellTotals { get; set; }
  }
}
