using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Catalog
{
  [Module(ModuleName = nameof(Common.Configuration.Modules.Catalog), OnDemand = true)]
  [ModuleDependency(nameof(Common.Configuration.Modules.BusinessLogic))]
  public class CatalogModule : IModule
  {
    #region IModule Members

    public IUnityContainer Container { get; private set; }
    public IRegionManager RegionManager { get; private set; }
    private INavigationService navigationService;

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
