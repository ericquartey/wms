using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BLL.Interfaces.Base
{
    public interface IReadAllPagedAsyncProvider<T>
    {
        #region Methods

        Task<IEnumerable<T>> GetAllAsync(
            int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null);

        Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            IExpression searchExpression = null);

        #endregion
    }
}
