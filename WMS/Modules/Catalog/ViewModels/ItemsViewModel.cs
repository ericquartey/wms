using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemsViewModel : BaseNavigationViewModel
  {
    private readonly IItemsService itemService;
    private IEnumerable<IItem> items;

    public IEnumerable<IItem> Items
    {
      get => this.items;
      set => this.SetProperty(ref this.items, value);
    }

    private IItem selectedItem;
    public IItem SelectedItem
    {
      get => this.selectedItem;
      set
      {
        if (this.SetProperty(ref this.selectedItem, value))
        {
          this.NotifySelectionChanged();
        }
      }
    }

    public ItemsViewModel(IItemsService itemService)
    {
      this.itemService = itemService;

      this.InitializeData();
    }

    #region Methods
    private void InitializeData()
    {
      this.Items = this.itemService.GetAll();
    }

    public void NotifySelectionChanged()
    {
      var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
      var navigationCompletedEvent = eventAggregator.GetEvent<ItemSelectionChangedEvent>();
      navigationCompletedEvent.Publish(this.selectedItem);
    }
    #endregion
  }
}
