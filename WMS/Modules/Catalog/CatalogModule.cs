using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Catalog
{
  [Module(ModuleName = nameof(Common.Utils.Modules.Catalog), OnDemand = true)]
  [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
  public class CatalogModule : IModule
  {
    #region IModule Members

    public IUnityContainer Container { get; private set; }
    public IRegionManager RegionManager { get; private set; }
    private readonly INavigationService navigationService;

    public CatalogModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
    {
      this.Container = container;
      this.RegionManager = regionManager;
      this.navigationService = navigationService;
    }

    public void Initialize()
    {
      this.navigationService.Register<ItemsView, ItemsViewModel>();
      this.navigationService.Register<ItemDetailsView, ItemDetailsViewModel>();
      this.navigationService.Register<ItemsAndDetailsView, ItemsAndDetailsViewModel>();
    }

    #endregion
  }
}
