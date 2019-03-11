using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IDeleteAsyncProvider<TModel, in TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<TModel>> DeleteAsync(TKey id);

        #endregion
    }
}
