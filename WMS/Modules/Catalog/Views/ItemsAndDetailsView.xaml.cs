using System.Windows.Controls;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemsAndDetailsView : UserControl
  {
    public ItemsAndDetailsView()
    {
      InitializeComponent();

      // TODO: This will be dynamic. See https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/43
      //this.DataContext = new ItemsAndDetailsViewModel(itemsService);
    }
  }
}
