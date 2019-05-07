using Ferretto.VW.AutomationService.Hubs;

namespace Ferretto.VW.AutomationService.Contracts
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
