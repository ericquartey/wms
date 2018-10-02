using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSourceService
    {
        #region Methods

        IEnumerable<IDataSource<IBusinessObject>> GetAll(string viewName, object parameter = null);

        #endregion Methods
    }
}
