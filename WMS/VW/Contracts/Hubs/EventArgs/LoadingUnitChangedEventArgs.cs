namespace Ferretto.VW.AutomationService.Contracts
{
    public class LoadingUnitChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public LoadingUnitChangedEventArgs(int? loadingUnitId)
        {
            this.LoadingUnitId = loadingUnitId;
        }

        #endregion

        #region Properties

        public int? LoadingUnitId { get; }

        #endregion
    }
}
