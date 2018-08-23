using System.Collections.ObjectModel;

namespace Ferretto.Common.Controls
{
  public class TileNavMenuChildItem
  {
    public string Info { get; set; }

    public ObservableCollection<NavMenuItem> Children { get; set; }
    public TileNavMenuChildItem(string parentInfo)
    {
      this.Info = parentInfo;
      this.Children = new ObservableCollection<NavMenuItem>();
    }
  }
}
