using Ferretto.VW.AutomationService.Hubs;

namespace Ferretto.VW.AutomationService.Contracts
{
    public class ModeChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ModeChangedEventArgs(int machineId, MachineMode mode, int? faultCode)
        {
            this.MachineId = machineId;
            this.FaultCode = faultCode;
            this.Mode = mode;
        }

        #endregion

        #region Properties

        public int? FaultCode { get; private set; }

        public int MachineId { get; private set; }

        public MachineMode Mode { get; }

        #endregion
    }
}
