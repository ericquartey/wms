using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
  // Stato di Riga di Lista Articoli
  public sealed class ItemListRowStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public IEnumerable<ItemListRow> ItemListRows { get; set; }
  }
}
