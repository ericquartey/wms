using System;
using System.Linq;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IFilterDataSource<TModel>
        where TModel : IBusinessObject
    {
        #region Properties

        string Expression { get; }

        Func<IQueryable<TModel>> GetData { get; }

        Func<int> GetDataCount { get; }

        string Key { get; }

        string Name { get; }

        IPagedBusinessProvider<TModel> Provider { get; }

        #endregion
    }
}
