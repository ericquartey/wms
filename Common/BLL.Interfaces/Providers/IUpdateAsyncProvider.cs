using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IUpdateAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<TModel>> UpdateAsync(TModel model);

        #endregion
    }
}
