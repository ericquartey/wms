using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces.Base
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
