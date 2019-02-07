using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadSingleAsyncProvider<TModel, in TKey>
    {
        #region Methods

        Task<TModel> GetByIdAsync(TKey id);

        #endregion
    }
}
