namespace Ferretto.VW.AutomationService.Contracts
{
    public class UserChangedEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>UserChangedChangedEventArgs</c> class.
        /// </summary>
        /// <param name="bayId">The id of the bay on which the user changed.</param>
        /// <param name="userId">The id of the logged in user, or <c>null</c> if no user logged to the specified bay.</param>
        public UserChangedChangedEventArgs(int bayId, int? userId)
        {
            this.BayId = bayId;
            this.UserId = userId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the id of the bay on which the user changed.
        /// </summary>
        public int BayId { get; }

        /// <summary>
        /// Gets the id of the logged in user.
        /// If <c>null</c>, it means that no user is logged to the specified bay.
        /// </summary>
        public int? UserId { get; }

        #endregion
    }
}
