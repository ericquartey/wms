using System;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IFilterDataSource<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Properties

        string Expression { get; }

        Func<IQueryable<TModel>> GetData { get; }

        Func<int> GetDataCount { get; }

        string Key { get; }

        string Name { get; }

        IPagedBusinessProvider<TModel, TKey> Provider { get; }

        #endregion
    }
}
