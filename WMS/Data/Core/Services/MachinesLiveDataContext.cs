using System.Collections.Generic;
using Ferretto.VW.MachineAutomationService.Contracts;
using Ferretto.VW.MachineAutomationService.Hubs;

namespace Ferretto.WMS.Data.Core
{
    public class MachinesLiveDataContext : IMachinesLiveDataContext
    {
        #region Fields

        private readonly Dictionary<int, MachineStatus> machineStatuses = new Dictionary<int, MachineStatus>();

        #endregion

        #region Properties

        public IList<IMachineHubClient> MachineHubs { get; } = new List<IMachineHubClient>();

        #endregion

        #region Methods

        public MachineStatus GetMachineStatus(int machineId)
        {
            lock (this.machineStatuses)
            {
                if (this.machineStatuses.ContainsKey(machineId) == false)
                {
                    var newMachineStatus = new MachineStatus
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
