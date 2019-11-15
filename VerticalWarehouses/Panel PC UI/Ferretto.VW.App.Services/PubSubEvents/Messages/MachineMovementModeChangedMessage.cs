using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.App.Services
{
    public class MachineMovementModeChangedMessage
    {
        #region Constructors

        public MachineMovementModeChangedMessage(MachineMovementMode machineMovementMode)
        {
            this.MachineMovementMode = machineMovementMode;
        }

        #endregion

        #region Properties

        public MachineMovementMode MachineMovementMode { get; }

        #endregion
    }
}
