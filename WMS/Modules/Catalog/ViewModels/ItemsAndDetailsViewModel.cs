using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Catalog
{
  internal class ItemsAndDetailsViewModel : BaseNavigationViewModel
  {
    #region Fields

    private bool detailsViewVisibility;

    #endregion Fields

    #region Constructors

    public ItemsAndDetailsViewModel()
    {
      ServiceLocator.Current.GetInstance<IEventService>()
       .Subscribe((ShowDetailsEventArgs<IItem> eventArgs) =>
       {
         this.DetailsViewVisibility = eventArgs.IsDetailsViewVisible;
       });
    }

    #endregion Constructors

    #region Properties

    public bool DetailsViewVisibility
    {
      get => this.detailsViewVisibility;
      set => this.SetProperty(ref this.detailsViewVisibility, value);
    }

    #endregion Properties

    #region Methods

    protected override void OnAppear()
    {
      this.DetailsViewVisibility = false;
    }

    #endregion Methods
  }
}
