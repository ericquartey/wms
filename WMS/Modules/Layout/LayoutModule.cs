using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Layout
{
  [Module(ModuleName = nameof(Common.Configuration.Modules.Layout), OnDemand = true)]
  [ModuleDependency(nameof(Common.Configuration.Modules.BusinessLogic))]
  public class LayoutModule : IModule
  {
    #region IModule Members

    public IUnityContainer Container { get; private set; }
    public IRegionManager RegionManager { get; private set; }

    public LayoutModule(IUnityContainer container, IRegionManager regionManager)
    {
      this.Container = container;
      this.RegionManager = regionManager;
    }

    public void Initialize()
    {
      var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
      regionManager.RegisterViewWithRegion($"{nameof(Layout)}.{nameof(Ferretto.Common.Configuration.Modules.Layout.MainContent)}", typeof(LayoutView));
      regionManager.RegisterViewWithRegion($"{nameof(Layout)}.{nameof(Ferretto.Common.Configuration.Modules.Layout.Menu)}", typeof(MenuView));
    }

    #endregion 
  }
}
