using System;

namespace Ferretto.VW.App.Services
{
    public class UserAuthenticatedEventArgs : System.EventArgs
    {
        #region Constructors

        public UserAuthenticatedEventArgs(
            string userName,
            MAS.AutomationService.Contracts.UserAccessLevel accessLevel)
        {
            this.UserName = userName;
            this.AccessLevel = accessLevel;
        }

        #endregion

        #region Properties

        public MAS.AutomationService.Contracts.UserAccessLevel AccessLevel { get; }

        public string UserName { get; }

        #endregion
    }
}
