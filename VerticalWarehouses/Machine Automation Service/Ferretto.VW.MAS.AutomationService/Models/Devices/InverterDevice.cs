using System.Collections.Generic;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class InverterDevice : DeviceBase
    {
        #region Properties

        public IEnumerable<BitBase> ControlWordItems { get; set; }

        public IEnumerable<BitBase> StatusWordItems { get; set; }

        #endregion
    }
}
