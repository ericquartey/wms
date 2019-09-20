using System.Collections.Generic;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class InverterDevice : DeviceBase
    {
        #region Properties

        public IEnumerable<BitBase> ControlWords { get; set; }

        public IEnumerable<BitBase> DigitalIOs { get; set; }

        public IEnumerable<BitBase> StatusWords { get; set; }

        #endregion
    }
}
