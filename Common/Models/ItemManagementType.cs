using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Tipo Gestione Articolo
    public partial class ItemManagementType
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public List<Item> Items { get; set; }
    }
}
