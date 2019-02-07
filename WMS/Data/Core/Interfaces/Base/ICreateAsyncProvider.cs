using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface ICreateAsyncProvider<T>
    {
        #region Methods

        Task<OperationResult<T>> CreateAsync(T model);

        #endregion
    }
}
