using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class MachineErrorEventArgs
    {
        #region Constructors

        public MachineErrorEventArgs(MachineError machineError)
        {
            this.MachineError = machineError;
        }

        #endregion

        #region Properties

        public MachineError MachineError { get; private set; }

        #endregion
    }
}
