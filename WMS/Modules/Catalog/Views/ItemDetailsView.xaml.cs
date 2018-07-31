using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemDetailsView : UserControl
  {
    public ItemDetailsView()
    {
      InitializeComponent();
      DataContext = ServiceLocator.Current.GetInstance<IItemDetailsViewModel>();
    }
  }
}
