using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Udc
    public sealed class LoadingUnitStatus
    {
        #region Properties

        public string Description { get; set; }

        public string Id { get; set; }

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }

        #endregion Properties
    }
}
