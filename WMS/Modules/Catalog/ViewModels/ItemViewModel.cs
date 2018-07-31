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
        if (Items != value)
        {
          items = value;
          RaisePropertyChanged();
        }
      }
    }

    private Common.Models.Item selectedItem;
    public Common.Models.Item SelectedItem
    {
      get
      {
        return selectedItem;
      }
      set
      {
        if (selectedItem != value)
        {
          selectedItem = value;          
          RaisePropertyChanged();
          NotifySelectionChanged();
        }
      }
    }

    public ItemViewModel()
    {
      InitializeData();
    }

    #region Methods
    private void InitializeData()
    {
      var itemService = ServiceLocator.Current.GetInstance<IItemsService>();
      Items = itemService.GetItems();    
    }
    
    public void NotifySelectionChanged()
    {
      var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
      var navigationCompletedEvent = eventAggregator.GetEvent<ItemSelectionChangedEvent>();
      navigationCompletedEvent.Publish(selectedItem);
    }
    #endregion
  }
}
