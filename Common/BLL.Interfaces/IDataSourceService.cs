using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSourceService
    {
        #region Methods

        IEnumerable<IDataSource<TModel, TId>> GetAll<TModel, TId>(string viewName, object parameter = null) where TModel : IBusinessObject<TId>;

        #endregion Methods
    }
}
