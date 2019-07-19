using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class UserProvider : IUserProvider
    {
        public UserClaims Authenticate(string userName, string password)
        {
            if (userName.ToUpper() == "OPERATOR"
                ||
                password == "password")
            {
                return new UserClaims
                {
                    Name = userName,
                    AccessLevel = UserAccessLevel.User,
                };
            }

            if (userName.ToUpper() == "INSTALLER"
               ||
               password == "password")
            {
                return new UserClaims
                {
                    Name = userName,
                    AccessLevel = UserAccessLevel.SuperUser,
                };
            }

            return null;
        }

        #region Methods

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult(Array.Empty<User>().AsEnumerable());
        }

        public Task<int> GetAllCountAsync()
        {
            return Task.FromResult(0);
        }

        public Task<User> GetByIdAsync(int id)
        {
            return Task.FromResult<User>(null);
        }

        public Task<bool> IsValidAsync(User user)
        {
            return Task.FromResult(true);
        }

        #endregion
    }
}
