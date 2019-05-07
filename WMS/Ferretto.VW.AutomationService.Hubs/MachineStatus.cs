using System.Collections.Generic;

namespace Ferretto.VW.AutomationService.Hubs
{
    public class MachineStatus
    {
        #region Properties

        private IEnumerable<BayStatus> Bays { get; set; }

        private ElevatorStatus Elevator { get; set; }

        private int? FaultCode { get; set; }

        private MachineMode Mode { get; set; }

        #endregion
    }
}
