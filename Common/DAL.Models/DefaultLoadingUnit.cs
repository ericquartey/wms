using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
    // Udc predefinite
    public sealed class DefaultLoadingUnit
    {
        public int Id { get; set; }
        public int LoadingUnitTypeId { get; set; }
        public Pairing CellPairing { get; set; }
        public string Image { get; set; }
        public int DefaultHandlingParametersCorrection { get; set; }

        public LoadingUnitType LoadingUnitType { get; set; }

        public IEnumerable<DefaultCompartment> DefaultCompartments { get; set; }
    }
}
