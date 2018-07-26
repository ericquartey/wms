using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Catalog
{
  [Module(ModuleName = nameof(Common.Configuration.Modules.Catalog), OnDemand = true)]
  [ModuleDependency(nameof(Common.Configuration.Modules.BusinessLogic))]
  public class Module : IModule
  {
    #region IModule Members

    public IUnityContainer Container { get; private set; }
    public IRegionManager RegionManager { get; private set; }

    public Module(IUnityContainer container, IRegionManager regionManager)
    {
      Container = container;
      RegionManager = regionManager;
    }

    public void Initialize()
    {
      var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();

      regionManager.RegisterViewWithRegion($"{nameof(Catalog)}.{nameof(Common.Configuration.Modules.Catalog.ItemsAndDetails)}", typeof(ItemsAndDetailsView));
      regionManager.RegisterViewWithRegion($"{nameof(Catalog)}.{nameof(Common.Configuration.Modules.Catalog.Items)}", typeof(ItemsView));
      regionManager.RegisterViewWithRegion($"{nameof(Catalog)}.{nameof(Common.Configuration.Modules.Catalog.ItemDetails)}", typeof(ItemDetailsView));
    }

    #endregion
  }
}
