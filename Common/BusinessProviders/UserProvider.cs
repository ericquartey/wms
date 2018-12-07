using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class UserProvider : IUserProvider
    {
        #region Methods

        public Task<Int32> Add(User model)
        {
            throw new NotImplementedException();
        }

        public Int32 Delete(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public Int32 GetAllCount()
        {
            throw new NotImplementedException();
        }

        public User GetById(Int32 id)
        {
            throw new NotImplementedException();
        }

        public string IsValid(User user)
        {
            if (user.Login.Equals("admin") &&
                user.Password.Equals("admin"))
            {
                return string.Empty;
            }

            return Resources.BusinessObjects.AccountAccessError;
        }

        public Int32 Save(User model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
