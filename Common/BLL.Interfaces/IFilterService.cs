using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IFilterService
  {
    IEnumerable<IFilter> GetByViewName(string viewName);
  }
}
