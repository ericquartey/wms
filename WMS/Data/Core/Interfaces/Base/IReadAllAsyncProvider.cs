using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadAllAsyncProvider<TModel, TKey>
        where TModel : BaseModel<TKey>
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAsync();

        Task<int> GetAllCountAsync();

        #endregion
    }
}
