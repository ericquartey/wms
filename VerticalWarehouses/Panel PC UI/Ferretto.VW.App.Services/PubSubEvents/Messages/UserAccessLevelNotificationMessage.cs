using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class UserAccessLevelNotificationMessage
    {
        #region Constructors

        public UserAccessLevelNotificationMessage(UserAccessLevel userAccessLevel)
        {
            this.UserAccessLevel = userAccessLevel;
        }

        #endregion

        #region Properties

        public UserAccessLevel UserAccessLevel { get; }

        #endregion
    }
}
