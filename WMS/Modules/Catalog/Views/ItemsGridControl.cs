using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemsGridControl : WmsGridControl<IItem, int>
  {
    #region Constructors

    public ItemsGridControl()
    {
      this.Initialize();
    }

    #endregion Constructors

    #region Methods

    private void Initialize()
    {
      ServiceLocator.Current.GetInstance<IEventService>()
        .Subscribe<ItemChangedEvent<IItem>>(eventArgs => this.Refresh(eventArgs.ChangedItem), true);
    }

    private void Refresh(IItem changedItem)
    {
      ((IWmsGridViewModel)this.DataContext)?.RefreshGrid();
    }

    #endregion Methods
  }
}
