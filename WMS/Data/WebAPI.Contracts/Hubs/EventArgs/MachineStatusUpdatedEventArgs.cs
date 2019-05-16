namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public class MachineStatusUpdatedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineStatusUpdatedEventArgs(VW.MachineAutomationService.Hubs.MachineStatus machineStatus)
        {
            this.MachineStatus = machineStatus;
        }

        #endregion

        #region Properties

        public VW.MachineAutomationService.Hubs.MachineStatus MachineStatus { get; }

        #endregion
    }
}
