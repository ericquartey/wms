using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Accessories
{
    public class BarcodeContext
    {
        #region Properties

        public string Name { get; set; }

        public IEnumerable<BarcodeRule> Rules { get; set; }

        #endregion
    }
}
