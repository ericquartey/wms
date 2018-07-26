using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.Generic;

namespace Ferretto.WMS.Comp.Catalog
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
      var items = new List<Item>();
      Item item = new Item() { Class = AbcClass.A, Id = 1, Description = "test", Height = 10, Width = 20 };
      Item item1 = new Item() { Class = AbcClass.A, Id = 2, Description = "test1", Height = 100, Width = 50 };
      items.Add(item);
      items.Add(item1);
      this.Items = items;

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
