using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Stato di Lista Articoli
    public partial class ItemListStatus
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public List<ItemList> ItemLists { get; set; }
    }
}
