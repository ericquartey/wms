using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IReadAllPagedAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null);

        Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null);

        #endregion
    }
}
