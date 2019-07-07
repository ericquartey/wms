using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IAuthenticationService
    {
        #region Properties

        /// <summary>
        /// Gets the name of the currently logged user, or <c>null</c> if no user is logged in.
        /// </summary>
        string UserName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Logs in the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>True if the user was successfully logged in, False otherwise.</returns>
        Task<bool> LogInAsync(string userName, string password);

        /// <summary>
        /// Logs out the user.
        /// </summary>
        Task LogOutAsync();

        #endregion
    }
}
