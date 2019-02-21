using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSourceService
    {
        #region Methods

        IEnumerable<IFilterDataSource<TModel, TKey>> GetAllFilters<TModel, TKey>(string viewModelName, object parameter = null)
            where TModel : IModel<TKey>;

        #endregion
    }
}
