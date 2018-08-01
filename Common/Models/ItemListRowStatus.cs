using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Stato di Riga di Lista Articoli
  public partial class ItemListRowStatus
  {
    public int Id { get; set; }
    public string Description { get; set; }

    public List<ItemListRow> ItemListRows { get; set; }
  }
}
