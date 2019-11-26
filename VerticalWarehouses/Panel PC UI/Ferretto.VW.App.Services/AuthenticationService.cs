using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    internal sealed class AuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly IMachineUsersWebService usersService;

        #endregion

        #region Constructors

        public AuthenticationService(IMachineUsersWebService usersService)
        {
            if (usersService is null)
            {
                throw new System.ArgumentNullException(nameof(usersService));
            }

            this.usersService = usersService;
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
            var userClaims = await this.usersService
                .AuthenticateWithResourceOwnerPasswordAsync(
                    userName,
                    password);

            this.UserName = userName;
            this.AccessLevel = userClaims.AccessLevel;
            this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userName, this.AccessLevel));

            return userClaims;
        }

        public Task LogOutAsync()
        {
            return Task.Run(() => this.UserName = null);
        }

        #endregion
    }
}
