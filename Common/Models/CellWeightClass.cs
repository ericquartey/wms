using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Classe Peso Cella
  public partial class CellWeightClass
  {
    public int Id { get; set; }
    public string Description { get; set; }
    public int MinWeight { get; set; }
    public int MaxWeight { get; set; }

    public List<CellType> CellTypes { get; set; }
  }
}
