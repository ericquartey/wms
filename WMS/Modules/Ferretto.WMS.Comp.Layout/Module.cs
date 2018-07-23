using Ferretto.Common.BLL;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Comp.Layout
{
  public class Layout : IModule
  {
    #region IModule Members

    public IUnityContainer Container { get; private set; }
    public IRegionManager RegionManager { get; private set; }

    public Layout(IUnityContainer container, IRegionManager regionManager)
    {
      Container = container;
      RegionManager = regionManager;
    }

    public void Initialize()
    {
      var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();

      regionManager.RegisterViewWithRegion($"{nameof(Modules.Layout)}.{nameof(Modules.Layout.MainContent)}", typeof(LayoutView));
      regionManager.RegisterViewWithRegion($"{nameof(Modules.Layout)}.{nameof(Modules.Layout.Menu)}", typeof(MenuView));
    }

    #endregion
  }
}
