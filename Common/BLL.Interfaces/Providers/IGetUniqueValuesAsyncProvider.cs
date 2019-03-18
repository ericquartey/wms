using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName);

        #endregion
    }
}
