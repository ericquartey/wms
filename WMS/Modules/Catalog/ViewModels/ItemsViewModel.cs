using System.Collections.Generic;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemsViewModel : BaseNavigationViewModel
  {
    #region Fields

    private readonly IFilterService filterService;

    private ICommand viewDetailsCommand;

    #endregion Fields

    #region Constructors

    public ItemsViewModel()
    {
      this.filterService = ServiceLocator.Current.GetInstance<IFilterService>();
    }

    #endregion Constructors

    #region Properties

    public IEnumerable<IFilter> Filters => this.filterService.GetByViewName("ItemsView");

    public ICommand ViewDetailsCommand => this.viewDetailsCommand ?? (this.viewDetailsCommand = new DelegateCommand(ExecuteViewDetailsCommand));

    #endregion Properties

    #region Methods

    private static void ExecuteViewDetailsCommand()
    {
      ServiceLocator.Current.GetInstance<IEventService>().Invoke(new ShowDetailsEventArgs<IItem>(true));
    }

    #endregion Methods
  }
}
