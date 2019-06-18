using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class UserProvider : IUserProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IUsersDataService usersDataService;

        #endregion

        #region Constructors

        public UserProvider(WMS.Data.WebAPI.Contracts.IUsersDataService usersDataService)
        {
            this.usersDataService = usersDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return (await this.usersDataService.GetAllAsync())
                    .Select(u => new User
                    {
                        Id = u.Id,
                        Login = u.Login,
                        Password = u.Password
                    });
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.usersDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                var user = await this.usersDataService.GetByIdAsync(id);
                return new User
                {
                    Id = user.Id,
                    Login = user.Login,
                    Password = user.Password
                };
            }
            catch
            {
                return null;
            }
        }

        public string IsValid(User user)
        {
            return string.Empty;
        }

        #endregion
    }
}
