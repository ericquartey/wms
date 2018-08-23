using System.Collections.ObjectModel;

namespace Ferretto.Common.Controls
{
  public class NavMenuViewModel
  {
    public NavMenuViewModel()
    {
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
