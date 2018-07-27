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
      this.Container.RegisterType<IItemDetailsViewModel, ItemDetailsViewModel>();
      this.Container.RegisterType<IItemViewModel, ItemViewModel>();            
      var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();            
      regionManager.RegisterViewWithRegion($"{nameof(Modules.Catalog)}.{nameof(Modules.Catalog.ItemsAndDetails)}", typeof(ItemsAndDetailsView));
    }

    #endregion
  }
}
