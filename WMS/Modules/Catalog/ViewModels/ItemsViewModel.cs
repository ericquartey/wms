using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemsViewModel : BaseNavigationViewModel
  {
    private readonly IFilterService filterService;

    public IEnumerable<IFilter> Filters
    {
      get { return this.filterService.GetByViewName("ItemsView"); }
    }

    public ItemsViewModel(IFilterService filterService)
    {
      this.filterService = filterService;
    }
  }
}
