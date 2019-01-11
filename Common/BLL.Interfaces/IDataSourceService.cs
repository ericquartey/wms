using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSourceService
    {
        #region Methods

        IEnumerable<IFilterDataSource<TModel>> GetAllFilters<TModel>(string viewModelName, object parameter = null)
            where TModel : IBusinessObject;

        #endregion Methods
    }
}
