using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.Modules.Compartment
{
    [Module(ModuleName = nameof(Common.Utils.Modules.Compartment), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class CompartmentModule : IModule
    {
        #region Constructors

        public CompartmentModule(IUnityContainer container, IRegionManager regionManager)
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
            this.splashScreenService.SetMessage("Initializing Compartment module ...");

            this.Container.RegisterType<INavigationService, NavigationService>(
                                        new ContainerControlledLifetimeManager());
            this.Container.RegisterType<IDialogService, Common.Controls.Services.DialogService>(new ContainerControlledLifetimeManager());

            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Compartment)}.{Common.Utils.Modules.Compartment.REGION_MAINCONTENT}",
                typeof(LayoutView));

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Register<LayoutView, LayoutViewModel>();
            navigationService.Register<CompartmentView, CompartmentViewModel>();
        }

        #endregion Methods
    }
}
