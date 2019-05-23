namespace Ferretto.VW.MachineAutomationService.Contracts
{
    public class LoadingUnitChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public LoadingUnitChangedEventArgs(int machineId, int? loadingUnitId)
        {
            this.MachineId = machineId;
            this.LoadingUnitId = loadingUnitId;
        }

        #endregion

        #region Properties

        public int? LoadingUnitId { get; }

        public int MachineId { get; private set; }

        #endregion
    }
}
