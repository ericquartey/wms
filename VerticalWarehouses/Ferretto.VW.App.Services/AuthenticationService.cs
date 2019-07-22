using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly IUsersDataService usersDataService;

        #endregion

        #region Constructors

        public AuthenticationService(IUsersDataService usersDataService)
        {
            if (usersDataService == null)
            {
                throw new System.ArgumentNullException(nameof(usersDataService));
            }

            this.usersDataService = usersDataService;
        }

        #endregion

        #region Events

        public event System.EventHandler<UserAuthenticatedEventArgs> UserAuthenticated;

        #endregion

        #region Properties

        public UserAccessLevel AccessLevel { get; private set; }

        public string UserName { get; private set; }

        #endregion

        #region Methods

        public async Task<UserClaims> LogInAsync(string userName, string password)
        {
            try
            {
                var userClaims = await this.usersDataService
                    .AuthenticateWithResourceOwnerPasswordAsync(
                        userName,
                        password);

                this.UserName = userName;
                this.AccessLevel = userClaims.AccessLevel;
                this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userName, userClaims.AccessLevel));

                return userClaims;
            }
            catch
            {
                return null;
            }
        }

        public Task LogOutAsync()
        {
            return Task.Run(() => this.UserName = null);
        }

        #endregion
    }
}
