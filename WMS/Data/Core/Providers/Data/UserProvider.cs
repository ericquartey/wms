using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class UserProvider : IUserProvider
    {
        #region Methods

        public UserClaims Authenticate(string userName, string password)
        {
            switch (userName.ToUpper())
            {
                case "OPERATOR":

                    return new UserClaims
                    {
                        Name = userName,
                        AccessLevel = Enums.UserAccessLevel.User,
                    };

                case "ADMIN":
                    return new UserClaims
                    {
                        Name = userName,
                        AccessLevel = Enums.UserAccessLevel.Admin,
                    };

                case "INSTALLER":
                    return new UserClaims
                    {
                        Name = userName,
                        AccessLevel = Enums.UserAccessLevel.SuperUser,
                    };

                default:
                    return null;
            }
        }

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

        #endregion
    }
}
