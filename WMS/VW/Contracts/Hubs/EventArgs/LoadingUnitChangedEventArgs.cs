namespace Ferretto.VW.MachineAutomationService.Contracts
{
    public class LoadingUnitChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public LoadingUnitChangedEventArgs(int machineId, int? bayId, int? loadingUnitId)
        {
            this.MachineId = machineId;
            this.LoadingUnitId = loadingUnitId;
            this.BayId = bayId;
        }

        #endregion

        #region Properties

        public int? BayId { get; }

        public int? LoadingUnitId { get; }

        public int MachineId { get; private set; }

        #endregion
    }
}
