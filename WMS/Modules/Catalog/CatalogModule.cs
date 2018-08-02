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

    public CatalogModule(IUnityContainer container, IRegionManager regionManager)
    {
      this.Container = container;
      this.RegionManager = regionManager;
    }

    public void Initialize()
    {
      this.Container.RegisterType<IItemDetailsViewModel, ItemDetailsViewModel>();
      this.Container.RegisterType<IItemViewModel, ItemViewModel>();            
      var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();            
      regionManager.RegisterViewWithRegion($"{nameof(Common.Configuration.Modules.Catalog)}.{nameof(Common.Configuration.Modules.Catalog.ItemDetails)}", typeof(ItemDetailsView));
      regionManager.RegisterViewWithRegion($"{nameof(Common.Configuration.Modules.Catalog)}.{nameof(Common.Configuration.Modules.Catalog.Items)}", typeof(ItemsView));
    }

    #endregion
  }
}
