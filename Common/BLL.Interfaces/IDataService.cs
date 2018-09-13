using System;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataService
    {
        IQueryable<TEntity> GetData<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate)
            where TEntity : class;
    }
}
