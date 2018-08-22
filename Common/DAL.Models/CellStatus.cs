using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
  // Stato Cella
  public sealed class CellStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public IEnumerable<Cell> Cells { get; set; }
    public IEnumerable<CellTotal> CellTotals { get; set; }
  }
}
