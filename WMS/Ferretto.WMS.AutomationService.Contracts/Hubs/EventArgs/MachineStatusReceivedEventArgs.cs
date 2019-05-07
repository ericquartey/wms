using Ferretto.VW.AutomationService.Hubs;

namespace Ferretto.VW.AutomationService.Contracts
{
    public class MachineStatusReceivedEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>MachineStatusReceivedEventArgs</c> class.
        /// </summary>
        /// <param name="machineStatus">The machine status.</param>
        public MachineStatusReceivedEventArgs(MachineStatus machineStatus)
        {
            this.MachineStatus = machineStatus;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the machine status.
        /// </summary>
        public MachineStatus MachineStatus { get; }

        #endregion
    }
}
