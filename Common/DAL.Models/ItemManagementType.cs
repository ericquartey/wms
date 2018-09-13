using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Tipo Gestione Articolo
    public sealed class ItemManagementType
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<Item> Items { get; set; }
    }
}
