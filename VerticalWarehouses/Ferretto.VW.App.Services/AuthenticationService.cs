using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        #region Properties

        public string UserName { get; private set; }

        #endregion

        #region Methods

        public async Task<bool> LogInAsync(string userName, string password)
        {
            var isLoginSuccessful = false;
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
