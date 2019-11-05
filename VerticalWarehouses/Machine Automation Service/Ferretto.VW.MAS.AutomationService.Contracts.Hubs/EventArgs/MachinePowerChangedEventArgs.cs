namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class MachinePowerChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachinePowerChangedEventArgs(MachinePowerState machinePowerState)
        {
            this.MachinePowerState = machinePowerState;
        }

        #endregion

        #region Properties

        public MachinePowerState MachinePowerState { get; }

        #endregion
    }
}
