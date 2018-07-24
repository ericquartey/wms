using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Stato Materiale
    public partial class MaterialStatus
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public List<Compartment> Compartments { get; set; }
        public List<ItemListRow> ItemListRows { get; set; }
    }
}
