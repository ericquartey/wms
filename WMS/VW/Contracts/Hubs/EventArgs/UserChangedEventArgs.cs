namespace Ferretto.VW.MachineAutomationService.Contracts
{
    public class UserChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public UserChangedEventArgs(int bayId, int? userId)
        {
            this.BayId = bayId;
            this.UserId = userId;
        }

        #endregion

        #region Properties

        public int BayId { get; }

        public int? UserId { get; }

        #endregion
    }
}
