using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.Core.Interfaces.Base
{
    public interface IReadAllPagedProvider<T>
    {
        #region Methods

        Task<IEnumerable<T>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<T, bool>> whereExpression = null,
            Expression<Func<T, bool>> searchExpression = null);

        Task<int> GetAllCountAsync(
            Expression<Func<T, bool>> whereExpression = null,
            Expression<Func<T, bool>> searchExpression = null);

        #endregion Methods
    }
}
