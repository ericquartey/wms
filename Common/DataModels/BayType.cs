using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Baia
    public sealed class BayType
    {
        #region Properties

        public List<Bay> Bays { get; }

        public string Description { get; set; }

        public string Id { get; set; }

        #endregion Properties
    }
}
