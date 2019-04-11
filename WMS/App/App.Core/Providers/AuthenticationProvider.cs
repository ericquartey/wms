using System;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class AuthenticationProvider : IAuthenticationProvider
    {
        #region Fields

        private readonly Data.WebAPI.Contracts.IAuthenticationService authenticationService;

        #endregion

        #region Constructors

        public AuthenticationProvider(Data.WebAPI.Contracts.IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        #endregion

        #region Methods

        public async Task<string> GetUserNameAsync()
        {
            return await this.authenticationService.GetUserNameAsync();
        }

        public async Task<IOperationResult<User>> LoginAsync(string userName, string password)
        {
            try
            {
                await this.authenticationService.LoginAsync(userName, password);

                return new OperationResult<User>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<User>(ex);
            }
        }

        #endregion
    }
}
