using System;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataService
    {
        #region Methods

        IQueryable<TEntity> GetData<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate = null)
            where TEntity : class;

        #endregion Methods
    }
}
