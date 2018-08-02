using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Classe Dimensione Udc
  public sealed class LoadingUnitSizeClass
  {
    public int Id { get; set; }
    public string Description { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
    public int? BayOffset { get; set; }
    public int? Lift { get; set; }
    public int? BayForksUnthread { get; set; }
    public int? CellForksUnthread { get; set; }

    public IEnumerable<LoadingUnitType> LoadingUnitTypes { get; set; }
  }
}
