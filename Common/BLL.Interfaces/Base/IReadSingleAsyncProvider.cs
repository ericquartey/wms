using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IReadSingleAsyncProvider<TModel, in TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<TModel> GetByIdAsync(TKey id);

        #endregion
    }
}
