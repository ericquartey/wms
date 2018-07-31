using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Scomparti predefiniti
  public partial class DefaultCompartment
  {
    public int Id { get; set; }
    public int DefaultLoadingUnitId { get; set; }
    public int CompartmentTypeId { get; set; }
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public string Image { get; set; }
    public string Note { get; set; }

    public DefaultLoadingUnit DefaultLoadingUnit { get; set; }
    public CompartmentType CompartmentType { get; set; }
  }
}
