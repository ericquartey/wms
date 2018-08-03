using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Stato Materiale
  public sealed class MaterialStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public IEnumerable<Compartment> Compartments { get; set; }
    public IEnumerable<ItemListRow> ItemListRows { get; set; }
    public IEnumerable<Mission> Missions { get; set; }
  }
}
