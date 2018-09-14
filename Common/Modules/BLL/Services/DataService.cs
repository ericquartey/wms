using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class DataService : IDataService
    {
        #region Fields

        private readonly DatabaseContext context;

        #endregion Fields

        #region Constructors

        public DataService(DatabaseContext context)
        {
            this.context = context;
        }

        #endregion Constructors

        #region Methods

        public IQueryable<TEntity> GetData<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate = null) where TEntity : class
        {
            if (predicate == null)
            {
                return this.context.Set<TEntity>();
            }
            return predicate.Invoke(this.context.Set<TEntity>());
        }

        public int SaveChanges() => this.context.SaveChanges();

        public Task<int> SaveChangesAsync() => this.context.SaveChangesAsync();

        #endregion Methods
    }
}
