using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.Generic;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemViewModel : BindableBase, IItemViewModel
  {

    private IEnumerable<object> items;
    public IEnumerable<object> Items
    {
      get
      {
        return items;
      }
      set
      {
        if (this.Items != value)
        {
          this.items = value;
          this.RaisePropertyChanged();
        }
      }
    }

    private object selectedItem;
    public object SelectedItem
    {
      get
      {
        return this.selectedItem;
      }
      set
      {
        if (this.selectedItem != value)
        {
          this.selectedItem = value;          
          this.RaisePropertyChanged();
          this.NotifySelectionChanged();
        }
      }
    }

    public ItemViewModel()
    {
      this.InitializeData();
    }

    #region Methods
    private void InitializeData()
    {
      var itemService = ServiceLocator.Current.GetInstance<IItemsService>();
      this.Items = itemService.GetItems();    
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
