using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;

namespace Ferretto.WMS.Comp.Catalog
{
  public partial class ItemDetailsView : UserControl
  {
    public ItemDetailsView()
    {
      InitializeComponent();
      this.DataContext = ServiceLocator.Current.GetInstance<IItemDetailsViewModel>();
    }
  }
}
