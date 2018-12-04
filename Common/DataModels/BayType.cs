using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Baia
    public sealed class BayType : IDataModel
    {
        #region Properties

        public List<Bay> Bays { get; set; }

        public string Description { get; set; }

        public string Id { get; set; }

        #endregion Properties
    }
}
