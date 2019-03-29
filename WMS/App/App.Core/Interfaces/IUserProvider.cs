using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
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
