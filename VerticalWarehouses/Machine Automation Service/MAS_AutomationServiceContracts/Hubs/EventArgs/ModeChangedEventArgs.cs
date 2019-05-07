using Ferretto.VW.AutomationService.Hubs;

namespace Ferretto.VW.AutomationService.Contracts
{
    public class ModeChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public MoeChangedEventArgs(MachineMode mode)
        {
            this.Mode = mode;
        }

        #endregion

        #region Properties

        public MachineMode Mode { get; }

        #endregion
    }
}
