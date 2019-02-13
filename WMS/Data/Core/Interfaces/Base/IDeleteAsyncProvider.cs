using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IDeleteAsyncProvider<T, in TKey>
    {
        #region Methods

        Task<OperationResult<T>> DeleteAsync(TKey id);

        #endregion
    }
}
