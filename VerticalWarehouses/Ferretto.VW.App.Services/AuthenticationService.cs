using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUsersDataService usersDataService;


        #region Events

        public event System.EventHandler<UserAuthenticatedEventArgs> UserAuthenticated;

        #endregion


        public AuthenticationService(IUsersDataService usersDataService)
        {
            if (usersDataService == null)
            {
                throw new System.ArgumentNullException(nameof(usersDataService));
            }

            this.usersDataService = usersDataService;
        }

        #region Properties

        public string UserName { get; private set; }

        #endregion

        #region Methods

        public async Task<bool> LogInAsync(string userName, string password)
        {
            var isLoginSuccessful = false;
            // TODO: uncomment when data service is released
            /*
            var loginSuccessful = await this.usersDataService.IsValidAsync(
                new User
                {
                    Login = this.UserLogin.UserName,
                    Password = this.UserLogin.Password
                });
              */


            switch (userName.ToUpperInvariant())
            {
                case "OPERATOR":
                    isLoginSuccessful = password == "password";
                    break;

                case "INSTALLER":
                    isLoginSuccessful = password == "password";
                    break;
            }

            if (isLoginSuccessful)
            {
                this.UserName = userName;
                this.UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs(userName));
            }

            await Task.Delay(500);

            return isLoginSuccessful;
        }

        public Task LogOutAsync()
        {
            return Task.Run(() => this.UserName = null);
        }

        #endregion
    }
}
