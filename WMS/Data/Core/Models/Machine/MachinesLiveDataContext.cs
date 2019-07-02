using System.Collections.Generic;
using Ferretto.VW.MachineAutomationService.Contracts;
using Ferretto.VW.MachineAutomationService.Hubs;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MachinesLiveDataContext : IMachinesLiveDataContext
    {
        #region Fields

        private readonly Dictionary<int, VW.MachineAutomationService.Hubs.MachineStatus> machineStatuses =
            new Dictionary<int, VW.MachineAutomationService.Hubs.MachineStatus>();

        #endregion

        #region Properties

        public IList<IMachineHubClient> MachineHubs { get; } = new List<IMachineHubClient>();

        #endregion

        #region Methods

        public VW.MachineAutomationService.Hubs.MachineStatus GetMachineStatus(int machineId)
        {
            lock (this.machineStatuses)
            {
                if (!this.machineStatuses.ContainsKey(machineId))
                {
                    var newMachineStatus = new VW.MachineAutomationService.Hubs.MachineStatus
                    {
                        MachineId = machineId,
                        ElevatorStatus = new ElevatorStatus(),
                        BaysStatus = new List<BayStatus>()
                    };

                    this.machineStatuses.Add(machineId, newMachineStatus);
                }
            }

            return this.machineStatuses[machineId];
        }

        #endregion
    }
}
