using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.WMS.Modules.Scheduler
{
    [Module(ModuleName = nameof(Common.Utils.Modules.Scheduler), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class SchedulerModule : IModule
    {
        #region Constructors

        public SchedulerModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.NavigationService = navigationService;
        }

        #endregion

        #region Properties

        public IUnityContainer Container { get; private set; }

        public INavigationService NavigationService { get; private set; }

        public IRegionManager RegionManager { get; private set; }

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            NLog.LogManager
                .GetCurrentClassLogger()
                .Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingSchedulerModule);

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Loading module ...");

            this.NavigationService.Register<MissionsView, MissionsViewModel>();
            this.NavigationService.Register<SchedulerRequestsView, SchedulerRequestsViewModel>();
        }

        #endregion
    }
}
