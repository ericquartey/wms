using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Classe Altezza Udc
  public sealed class LoadingUnitHeightClass
  {
    public int Id { get; set; }
    public string Description { get; set; }
    public int MinHeight { get; set; }
    public int MaxHeight { get; set; }

    public List<LoadingUnitType> LoadingUnitTypes { get; set; }
  }
}
