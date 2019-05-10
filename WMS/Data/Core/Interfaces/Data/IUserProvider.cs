using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IUserProvider :
        IReadAllAsyncProvider<User, int>,
        IReadSingleAsyncProvider<User, int>
    {
        #region Methods

        Task<bool> IsValidAsync(User user);

        #endregion
    }
}
