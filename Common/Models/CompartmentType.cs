using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Tipo Scomparto
  public sealed class CompartmentType
  {
    public int Id { get; set; }
    public string Description { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    public IEnumerable<Compartment> Compartments { get; set; }
    public IEnumerable<DefaultCompartment> DefaultCompartments { get; set; }
  }
}
