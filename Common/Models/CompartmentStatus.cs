using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Scomparto
    public sealed class CompartmentStatus
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<Compartment> Compartments { get; set; }
    }
}
