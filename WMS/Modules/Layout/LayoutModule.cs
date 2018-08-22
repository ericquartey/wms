using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Layout
{
  [Module(ModuleName = nameof(Common.Utils.Modules.Layout), OnDemand = true)]
  [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
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
      this.Container.RegisterType<INavigationService, NavigationService>(new ContainerControlledLifetimeManager());
      this.Container.RegisterType<IDialogService, DialogService>(new ContainerControlledLifetimeManager());

      var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
      navigationService.Register<LayoutView, LayoutViewModel>();
      navigationService.Register<MenuView, MenuViewModel>();

      this.RegionManager.RegisterViewWithRegion($"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MAINCONTENT}", typeof(LayoutView));
      this.RegionManager.RegisterViewWithRegion($"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MENU}", typeof(MenuView));
    }

    #endregion 
  }
}
