using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface ICreateAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IOperationResult<TModel>> CreateAsync(TModel model);

        #endregion
    }
}
