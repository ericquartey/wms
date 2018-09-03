using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL.Services
{
  public class FilterService : IFilterService
  {
    public IEnumerable<IFilter> GetByViewName(string viewName)
    {
      return new List<Filter> {
        new Filter {Id= 1, Name = "All", Count = 53 },
        new Filter { Id = 2, Name = "Category 1", Count  = 7 },
        new Filter { Id = 3, Name = "Category 2", Count  = 21 }
      };
    }
  }
}
