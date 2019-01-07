using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class UserProvider : IUserProvider
    {
        #region Methods

        public Task<OperationResult> AddAsync(User model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            throw new NotImplementedException();
        }

        public Task<User> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public string IsValid(User user)
        {
            return string.Empty;
        }

        public Task<OperationResult> SaveAsync(User model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
