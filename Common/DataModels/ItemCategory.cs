using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Categoria Merceologica Articolo
    public sealed class ItemCategory
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<Item> Items { get; set; }
    }
}
