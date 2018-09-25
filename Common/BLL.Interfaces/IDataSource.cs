using System;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSource<TEntity> where TEntity : IBusinessObject
    {
        #region Properties

        int Count { get; }
        Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }
        string Name { get; }

        #endregion Properties

        #region Methods

        IQueryable<TEntity> Load();

        #endregion Methods
    }
}
