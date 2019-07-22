using System;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class UserAuthenticatedEventArgs : EventArgs
    {
        #region Constructors

        public UserAuthenticatedEventArgs(string userName, UserAccessLevel accessLevel)
        {
            this.UserName = userName;
            this.AccessLevel = accessLevel;
        }

        #endregion

        #region Properties

        public UserAccessLevel AccessLevel { get; }

        public string UserName { get; }

        #endregion
    }
}
