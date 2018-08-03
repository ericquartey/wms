using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Models;

namespace Ferretto.WMS.Modules.Catalog
{
  class ItemsAndDetailsViewModel: BaseNavigationViewModel
  {
    private readonly IItemsService itemsService;
    
    public ItemsAndDetailsViewModel(IItemsService itemsService)
    {
      this.itemsService = itemsService;
    }

    public IEnumerable<Item> Items
    {
      get
      {
        return this.itemsService.GetItems().ToList();
      }
    }
  }
}
