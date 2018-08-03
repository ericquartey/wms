using System.Windows.Input;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Layout
{
  public class MenuViewModel : BaseViewModel
  {
    private ICommand cmdItem;
    public ICommand CmdItem
    {
      get { return this.cmdItem ?? (this.cmdItem = new DelegateCommand(this.AppearItem)); }
    }

    INavigationService navigationService;
    public MenuViewModel(INavigationService navigationService)
    {
      this.navigationService = navigationService;
    }

    private void AppearItem()
    {
      this.navigationService.Appear(nameof(Common.Utils.Modules.Catalog), Common.Utils.Modules.Catalog.ITEMSANDDETAILS);
    }
  }
}
