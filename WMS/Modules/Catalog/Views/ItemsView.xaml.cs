using System.Windows.Input;
using DevExpress.Xpf.Grid;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemsView : WmsView
  {
    #region Constructors

    public ItemsView()
    {
      this.InitializeComponent();

      this.DataContext = new ItemsViewModel();
    }

    #endregion
  }
}
