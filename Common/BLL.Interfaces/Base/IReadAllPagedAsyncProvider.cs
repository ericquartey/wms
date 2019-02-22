using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IReadAllPagedAsyncProvider<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBy = null,
            string whereExpression = null,
            string searchString = null);

        Task<int> GetAllCountAsync(
            string whereExpression = null,
            string searchString = null);

        #endregion
    }
}
