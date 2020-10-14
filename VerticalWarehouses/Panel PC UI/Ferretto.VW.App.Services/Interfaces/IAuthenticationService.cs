using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IAuthenticationService
    {
        #region Events

        event System.EventHandler<UserAuthenticatedEventArgs> UserAuthenticated;

        #endregion

        #region Properties

        bool IsAutoLogoutServiceUser { get; set; }

        /// <summary>
        /// Gets the name of the currently logged user, or <c>null</c> if no user is logged in.
        /// </summary>
        string UserName { get; }

        #endregion

        #region Methods

        Task<string> GetToken();

        /// <summary>
        /// Logs in the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>The user claims if the user was successfully logged in, <c>null</c> otherwise.</returns>
        Task<UserClaims> LogInAsync(string userName, string password, string supportToken, UserAccessLevel accessLevel = UserAccessLevel.NoAccess);

        Task<UserClaims> LogInAsync(string bearerToken);

        /// <summary>
        /// Logs out the user.
        /// </summary>
        Task LogOutAsync();

        #endregion
    }
}
