using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface ICreateAsyncProvider<T>
    {
        #region Methods

        Task<IOperationResult> CreateAsync(T model);

        #endregion
    }
}
