using System.Collections.Generic;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.AutomationService
{
    public class InverterParameterSet
    {
        #region Properties

        public byte Index { get; set; }

        public IEnumerable<InverterParameter> Parameters { get; set; }

        #endregion
    }
}
