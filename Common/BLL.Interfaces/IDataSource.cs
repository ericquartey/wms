using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSource<TEntity> where TEntity : IBusinessObject
    {
        #region Properties

        int Count { get; }
        Func<IEnumerable<TEntity>, IEnumerable<TEntity>> Filter { get; }
        string Name { get; }

        #endregion Properties

        #region Methods

        IEnumerable<TEntity> Load();

        #endregion Methods
    }
}
