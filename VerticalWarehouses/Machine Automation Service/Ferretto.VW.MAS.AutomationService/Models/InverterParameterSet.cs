using System.Collections.Generic;

namespace Ferretto.VW.MAS.AutomationService
{
    public class InverterParameterSet
    {
        #region Properties

        public int Index { get; set; }

        public IEnumerable<InverterParameter> Parameters { get; set; }

        #endregion
    }
}
