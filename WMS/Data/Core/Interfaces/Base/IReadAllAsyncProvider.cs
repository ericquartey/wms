using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadAllAsyncProvider<T>
    {
        #region Methods

        Task<IEnumerable<T>> GetAllAsync();

        Task<int> GetAllCountAsync();

        #endregion
    }
}
