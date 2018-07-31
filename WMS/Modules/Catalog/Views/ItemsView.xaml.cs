using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemsView : UserControl
  {
    public ItemsView()
    {
      InitializeComponent();      
      DataContext = ServiceLocator.Current.GetInstance<IItemViewModel>();
    }
  }
}
