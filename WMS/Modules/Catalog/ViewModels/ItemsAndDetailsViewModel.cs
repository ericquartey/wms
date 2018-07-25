using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Models;
using Prism.Mvvm;

namespace Ferretto.WMS.Modules.Catalog
{
  class ItemsAndDetailsViewModel: BindableBase
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
