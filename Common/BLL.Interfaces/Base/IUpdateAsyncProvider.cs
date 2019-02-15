using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IUpdateAsyncProvider<T>
    {
        #region Methods

        Task<IOperationResult> UpdateAsync(T model);

        #endregion
    }
}
