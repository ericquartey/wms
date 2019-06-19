namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public class MachineStatusUpdatedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineStatusUpdatedEventArgs(Hubs.Models.MachineStatus machineStatus)
        {
            this.MachineStatus = machineStatus;
        }

        #endregion

        #region Properties

        public Hubs.Models.MachineStatus MachineStatus { get; }

        #endregion
    }
}
