using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Udc predefinite
    public partial class DefaultLoadingUnit
    {
        public int Id { get; set; }
        public int LoadingUnitTypeId { get; set; }
        public Pairing CellPairing { get; set; }
        public string Image { get; set; }
        public int DefaultHandlingParametersCorrection { get; set; }

        public LoadingUnitType LoadingUnitType { get; set; }

        public List<DefaultCompartment> DefaultCompartments { get; set; }
    }
}
