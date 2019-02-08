using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IUpdateAsyncProvider<T>
    {
        #region Methods

        Task<OperationResult<T>> UpdateAsync(T model);

        #endregion
    }
}
