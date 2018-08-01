using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Stato Scomparto
  public partial class CompartmentStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public List<Compartment> Compartments { get; set; }
  }
}
