using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSourceService
    {
        #region Methods

        IEnumerable<IDataSource<TModel>> GetAll<TModel>(string viewName, object parameter = null) where TModel : IBusinessObject;

        #endregion Methods
    }
}
