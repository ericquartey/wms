using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class IoDevice : DeviceBase
    {
        #region Properties

        public IEnumerable<BitInfo> Inputs { get; set; }

        public IEnumerable<BitInfo> IoStatuses { get; set; }

        public IEnumerable<BitInfo> Outputs { get; set; }

        #endregion
    }
}
