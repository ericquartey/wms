using System;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.DAL.EF;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class DataService : IDataService
    {
        private readonly DatabaseContext context;

        public DataService(DatabaseContext context)
        {
            this.context = context;
        }

        public IQueryable<TEntity> GetData<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate = null) where TEntity : class
        {
            if (predicate == null)
            {
                return this.context.Set<TEntity>();
            }
            return predicate.Invoke(this.context.Set<TEntity>());
        }
    }
}
