using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class MachineErrorStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineErrorStatusChangedEventArgs(MachineError error)
        {
            this.Error = error;
        }

        #endregion

        #region Properties

        public MachineError Error { get; }

        #endregion
    }
}
