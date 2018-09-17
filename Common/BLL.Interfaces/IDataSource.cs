using System;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSource<TEntity> where TEntity : class
    {
        #region Properties

        int Count { get; }
        string Name { get; }
        Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }

        #endregion Properties

        #region Methods

        IQueryable<TEntity> Load();

        #endregion
    }
}
