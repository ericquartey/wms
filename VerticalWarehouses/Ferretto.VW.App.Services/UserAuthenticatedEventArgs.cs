using System;

namespace Ferretto.VW.App.Services
{
    public class UserAuthenticatedEventArgs : EventArgs
    {
        #region Constructors

        public UserAuthenticatedEventArgs(string userName)
        {
            this.UserName = userName;
        }

        #endregion

        #region Properties

        public string UserName { get; }

        #endregion
    }
}
