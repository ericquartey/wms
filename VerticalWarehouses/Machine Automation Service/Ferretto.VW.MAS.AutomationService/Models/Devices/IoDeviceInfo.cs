using System.Collections.Generic;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class IoDeviceInfo : DeviceBase
    {
        #region Properties

        public IEnumerable<BitInfo> Inputs { get; set; }

        public IEnumerable<BitInfo> IoStatuses { get; set; }

        public IEnumerable<BitInfo> Outputs { get; set; }

        #endregion
    }
}
