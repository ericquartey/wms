using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.WMS.Modules.Catalog
{
  public partial class ItemsAndDetailsView : WMSView
  {
    IDialogService dialogService;
    public ItemsAndDetailsView(IDialogService dialogService)
    {
      InitializeComponent();

      this.dialogService = dialogService;
    {
    }
  }
}
