using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IUserProvider :
        IReadSingleAsyncProvider<User, int>,
        IReadAllAsyncProvider<User>
    {
        #region Methods

        string IsValid(User user);

        #endregion
    }
}
