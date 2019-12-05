using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public class MachineStatusChangedMessage
    {
        #region Constructors

        public MachineStatusChangedMessage(MachineStatus machineStatus)
        {
            this.MachineStatus = machineStatus;
        }

        #endregion

        #region Properties

        public MachineStatus MachineStatus { get; }

        #endregion
    }
}
