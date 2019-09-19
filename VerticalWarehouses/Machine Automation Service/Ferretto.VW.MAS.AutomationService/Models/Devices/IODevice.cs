using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class IoDevice : DeviceBase
    {
        #region Properties

        public IEnumerable<BitBase> IoStatusItems { get; set; }

        #endregion
    }
}
