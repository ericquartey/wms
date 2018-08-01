using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Stato Udc
  public partial class LoadingUnitStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public List<LoadingUnit> LoadingUnits { get; set; }
  }
}
