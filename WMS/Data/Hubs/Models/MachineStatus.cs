using System.Collections.Generic;

namespace Ferretto.WMS.Data.Hubs.Models
{
    public class MachineStatus
    {
        #region Properties

        public IEnumerable<BayStatus> BaysStatus { get; set; }

        public ElevatorStatus ElevatorStatus { get; set; }

        public int? FaultCode { get; set; }

        public int MachineId { get; set; }

        public MachineMode Mode { get; set; } = MachineMode.Offline;

        #endregion
    }
}
