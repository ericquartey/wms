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
            return (await this.usersDataService.GetAllAsync())
                .Select(u => new User
                {
                    Id = u.Id,
                    Login = u.Login,
                    Password = u.Password
                });
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.usersDataService.GetAllCountAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var user = await this.usersDataService.GetByIdAsync(id);
            return new User
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password
            };
        }

        public string IsValid(User user)
        {
            return string.Empty;
        }

        #endregion
    }
}
