using System.Collections.Generic;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.VW.MachineAutomationService.Hubs
{
    public class MachineStatus
    {
        #region Properties

        public IEnumerable<BayStatus> BaysStatus { get; set; }

        public ElevatorStatus ElevatorStatus { get; set; }

        public int? FaultCode { get; set; }

        public int MachineId { get; set; }

        public Enums.MachineStatus Mode { get; set; } = Enums.MachineStatus.Offline;

        #endregion
    }
}
