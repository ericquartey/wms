using System.Collections.Generic;
using Ferretto.VW.AutomationService.Hubs;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public class LiveMachineDataContext : ILiveMachineDataContext
    {
        #region Fields

        private readonly MachineStatus machineStatus = new MachineStatus
        {
            ElevatorStatus = new ElevatorStatus(),
            BaysStatus = new List<BayStatus>(),
            Mode = MachineMode.Offline
        };

        #endregion

        #region Properties

        public MachineStatus MachineStatus => this.machineStatus;

        #endregion
    }
}
