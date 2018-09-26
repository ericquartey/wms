using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Gestione Articolo
    public sealed class ItemManagementType
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<Item> Items { get; set; }
    }
}
