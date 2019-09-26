using System.Collections.Generic;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class InverterDevice : DeviceBase
    {
        #region Properties

        public IEnumerable<BitInfo> ControlWords { get; set; }

        public IEnumerable<BitInfo> DigitalInputs { get; set; }

        public IEnumerable<BitInfo> StatusWords { get; set; }

        #endregion
    }
}
