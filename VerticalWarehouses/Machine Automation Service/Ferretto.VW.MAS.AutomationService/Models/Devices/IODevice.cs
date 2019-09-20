using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class IoDevice : DeviceBase
    {
        #region Properties

        public IEnumerable<BitBase> Inputs { get; set; }

        public IEnumerable<BitBase> IoStatuses { get; set; }

        public IEnumerable<BitBase> Outputs { get; set; }

        #endregion
    }
}
