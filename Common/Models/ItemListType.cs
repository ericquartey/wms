using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Tipo Lista Articoli
    public partial class ItemListType
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public List<ItemList> ItemLists { get; set; }
    }
}
