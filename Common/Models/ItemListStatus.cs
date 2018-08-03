using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Stato di Lista Articoli
  public sealed class ItemListStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public IEnumerable<ItemList> ItemLists { get; set; }
  }
}
