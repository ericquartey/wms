using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadAllPagedAsyncProvider<TModel, TKey>
        where TModel : BaseModel<TKey>
    {
        #region Methods

        Task<IEnumerable<TModel>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            IExpression whereExpression = null,
            Expression<Func<TModel, bool>> searchExpression = null);

        Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            Expression<Func<TModel, bool>> searchExpression = null);

        #endregion
    }
}
