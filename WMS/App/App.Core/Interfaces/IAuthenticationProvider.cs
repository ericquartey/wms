using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IAuthenticationProvider
    {
        #region Methods

        Task<string> GetUserNameAsync();

        Task<IOperationResult<User>> LoginAsync(string userName, string password);

        #endregion
    }
}
