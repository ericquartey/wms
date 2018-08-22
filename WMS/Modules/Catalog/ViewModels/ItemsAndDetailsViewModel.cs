using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Catalog
{
  class ItemsAndDetailsViewModel : BaseNavigationViewModel
  {
    private readonly IItemsService itemsService;

    public ItemsAndDetailsViewModel(IItemsService itemsService)
    {
      this.itemsService = itemsService;
    }

    public IEnumerable<IItem> Items => this.itemsService.GetAll();
  }
}
