using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Udc
    public sealed class LoadingUnitStatus : IDataModel<string>
    {
        #region Properties

        public string Description { get; set; }

        public string Id { get; set; }

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }

        #endregion
    }
}
