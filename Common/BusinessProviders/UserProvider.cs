using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class UserProvider : IUserProvider
    {
        #region Methods

        public Task<OperationResult> AddAsync(User model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<User> GetAll() => throw new NotSupportedException();

        public int GetAllCount() => throw new NotSupportedException();

        public Task<User> GetByIdAsync(int id) => throw new NotSupportedException();

        public string IsValid(User user)
        {
            return string.Empty;
        }

        public Task<OperationResult> SaveAsync(User model) => throw new NotSupportedException();

        #endregion
    }
}
