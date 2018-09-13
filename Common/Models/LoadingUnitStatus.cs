using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Stato Udc
    public sealed class LoadingUnitStatus
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }
    }
}
