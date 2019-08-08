using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.VW.MachineAutomationService.Contracts
{
    public class ModeChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ModeChangedEventArgs(int machineId, Enums.MachineStatus mode, int? faultCode)
        {
            this.MachineId = machineId;
            this.FaultCode = faultCode;
            this.Mode = mode;
        }

        #endregion

        #region Properties

        public int? FaultCode { get; private set; }

        public int MachineId { get; private set; }

        public Enums.MachineStatus Mode { get; }

        #endregion
    }
}
