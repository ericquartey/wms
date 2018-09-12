using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IFilterService
  {
    #region Methods

    IEnumerable<IFilter> GetByViewName(string viewName);

    #endregion Methods
  }
}
