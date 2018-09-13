using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataService
    {
        #region Methods

        IQueryable<TEntity> GetData<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate = null)
            where TEntity : class;

        Task<int> SaveChangesAsync();
        int SaveChanges();

        #endregion Methods
    }
}
