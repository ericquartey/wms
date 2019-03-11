using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IUpdateAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<TModel>> UpdateAsync(TModel model);

        #endregion
    }
}
