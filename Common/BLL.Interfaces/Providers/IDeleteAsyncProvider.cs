using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IDeleteAsyncProvider<TModel, in TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<TModel>> DeleteAsync(TKey id);

        #endregion
    }
}
