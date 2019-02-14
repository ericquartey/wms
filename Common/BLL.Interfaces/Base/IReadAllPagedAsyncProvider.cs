using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            IExpression whereExpression = null,
            Expression<Func<TModel, bool>> searchExpression = null);

        Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            Expression<Func<TModel, bool>> searchExpression = null);

        #endregion
    }
}
