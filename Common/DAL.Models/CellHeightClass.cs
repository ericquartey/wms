using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
  // Classe Altezza Cella
  public sealed class CellHeightClass
  {
    public int Id { get; set; }
    public string Description { get; set; }
    public int MinHeight { get; set; }
    public int MaxHeight { get; set; }

    public IEnumerable<CellType> CellTypes { get; set; }
  }
}
