using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemDetailsView : WmsView
  {
    public ItemDetailsView()
    {
      this.InitializeComponent();

      this.DataContext = new ItemDetailsViewModel();
    }
    
  }
}
