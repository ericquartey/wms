using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IUserProvider :
        IReadAllAsyncProvider<User, int>,
        IReadSingleAsyncProvider<User, int>
    {
        Task<bool> IsValidAsync(User user);
    }
}
