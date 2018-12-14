using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class UserProvider : IUserProvider
    {
        #region Methods

        public Task<OperationResult> Add(User model)
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
            return string.Empty;
        }

        public Int32 Save(User model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
