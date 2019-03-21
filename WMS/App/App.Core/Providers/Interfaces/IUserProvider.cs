using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IUserProvider :
        IReadSingleAsyncProvider<User, int>,
        IReadAllAsyncProvider<User, int>
    {
        #region Methods

        string IsValid(User user);

        #endregion
    }
}
