using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using System.Linq;

namespace Ferretto.WMS.Modules.Layout
{
    [Module(ModuleName = nameof(Common.Utils.Modules.Layout), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class LayoutModule : IModule
    {
        #region Constructors

        public LayoutModule(IUnityContainer container, IRegionManager regionManager)
        {
            this.Container = container;
            this.RegionManager = regionManager;
        }

        #endregion Constructors

        #region Properties

        public IUnityContainer Container { get; private set; }
        public IRegionManager RegionManager { get; private set; }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            this.Container.RegisterType<INavigationService, NavigationService>(
                new ContainerControlledLifetimeManager());
            this.Container.RegisterType<IDialogService, DialogService>(new ContainerControlledLifetimeManager());

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Register<LayoutView, LayoutViewModel>();
            navigationService.Register<MenuView, MenuViewModel>();

            // TODO: review this call to ensure we do a proper initialization of the entity framework
            ServiceLocator.Current.GetInstance<IDataService>().GetData<Item>().FirstOrDefault(item => item.Id == 4);

            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MAINCONTENT}",
                typeof(LayoutView));
            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MENU}", typeof(MenuView));
        }

        #endregion Methods
    }
}
