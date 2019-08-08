using System.Collections.Generic;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Hubs.Models
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
