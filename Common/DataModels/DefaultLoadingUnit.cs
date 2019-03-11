using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Udc predefinite
    public sealed class DefaultLoadingUnit : IDataModel
    {
        #region Properties

        public IEnumerable<DefaultCompartment> DefaultCompartments { get; set; }

        public int DefaultHandlingParametersCorrection { get; set; }

        public int Id { get; set; }

        public string Image { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public LoadingUnitType LoadingUnitType { get; set; }

        public int LoadingUnitTypeId { get; set; }

        #endregion
    }
}
