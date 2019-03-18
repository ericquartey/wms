using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IReadSingleAsyncProvider<TModel, in TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<TModel> GetByIdAsync(TKey id);

        #endregion
    }
}
