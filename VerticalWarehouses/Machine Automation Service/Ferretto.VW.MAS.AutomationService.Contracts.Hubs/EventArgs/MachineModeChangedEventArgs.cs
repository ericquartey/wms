namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class MachineModeChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineModeChangedEventArgs(MachineMode machineMode)
        {
            this.MachineMode = machineMode;
        }

        #endregion

        #region Properties

        public MachineMode MachineMode { get; }

        #endregion
    }
}
