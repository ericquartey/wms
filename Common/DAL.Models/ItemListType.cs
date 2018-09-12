using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
    // Tipo Lista Articoli
    public sealed class ItemListType
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<ItemList> ItemLists { get; set; }
    }
}
