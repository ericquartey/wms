using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IReadAllAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAsync();

        Task<int> GetAllCountAsync();

        #endregion
    }
}
