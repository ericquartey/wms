namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class AssignedMissionOperationChangedEventArgs : System.EventArgs, IBayEventArgs
    {
        #region Constructors

        public AssignedMissionOperationChangedEventArgs(
            BayNumber bayNumber)
        {
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        #endregion
    }
}
