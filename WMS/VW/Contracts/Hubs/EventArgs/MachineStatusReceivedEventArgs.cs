using Ferretto.VW.MachineAutomationService.Hubs;

namespace Ferretto.VW.MachineAutomationService.Contracts
{
    public class MachineStatusReceivedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineStatusReceivedEventArgs(MachineStatus machineStatus)
        {
            this.MachineStatus = machineStatus;
        }

        #endregion

        #region Properties

        public MachineStatus MachineStatus { get; }

        #endregion
    }
}
