using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Classe Peso Udc
  public sealed class LoadingUnitWeightClass
  {
    public int Id { get; set; }
    public string Description { get; set; }
    public int MinWeight { get; set; }
    public int MaxWeight { get; set; }

    public List<LoadingUnitType> LoadingUnitTypes { get; set; }
  }
}
