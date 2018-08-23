using System;
using System.Collections.ObjectModel;
using Ferretto.Common.Utils.Menu;

namespace Ferretto.Common.Controls
{
  public class NavMenuViewModel
  {
    public NavMenuViewModel()
    {
      this.Inizialize();
    }

    private void Inizialize()
    {
      this.Items = new ObservableCollection<NavMenuItem>();
      var menu = new AppMenu();
      foreach (var item in menu.Menu.Items)
      {
        this.Items.Add(new NavMenuItem(item, string.Empty));
      }
    }

    public NavMenuViewModel(ObservableCollection<NavMenuItem> items)
    {
      this.Items = items;
    }

    public ObservableCollection<NavMenuItem> Items
    {
      get;
      set;
    }
  }
}
