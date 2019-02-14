using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class UserProvider : IUserProvider
    {
        #region Methods

        public Task<IOperationResult> AddAsync(User model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<User> GetAll() => throw new NotSupportedException();

        public int GetAllCount() => throw new NotSupportedException();

        public Task<User> GetByIdAsync(int id) => throw new NotSupportedException();

        public User GetNew()
        {
            throw new NotImplementedException();
        }

        public string IsValid(User user)
        {
            return string.Empty;
        }

        public Task<IOperationResult> SaveAsync(User model) => throw new NotSupportedException();

        #endregion
    }
}
