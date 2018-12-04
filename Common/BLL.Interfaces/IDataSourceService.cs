using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IDataSourceService
    {
        #region Methods

        IEnumerable<ITileDataSource<TModel>> GetAllTiles<TModel>(string viewModelName, object parameter = null) where TModel : IBusinessObject;

        #endregion Methods
    }
}
