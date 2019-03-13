using System;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IFilterDataSource<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Properties

        string FilterString { get; }

        Func<IQueryable<TModel>> GetData { get; }

        Func<int> GetDataCount { get; }

        string Key { get; }

        string Name { get; }

        IPagedBusinessProvider<TModel, TKey> Provider { get; }

        #endregion
    }
}
