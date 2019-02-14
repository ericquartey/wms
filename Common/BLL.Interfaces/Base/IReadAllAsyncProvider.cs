using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IReadAllAsyncProvider<T>
    {
        #region Methods

        Task<IEnumerable<T>> GetAllAsync();

        Task<int> GetAllCountAsync();

        #endregion
    }
}
