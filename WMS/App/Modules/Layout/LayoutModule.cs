using System;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;
using Unity.Lifetime;

namespace Ferretto.WMS.Modules.Layout
{
    [Module(ModuleName = nameof(Common.Utils.Modules.Layout), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
#pragma warning disable S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)
    public class LayoutModule : IModule
#pragma warning restore S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)
    {
        #region Constructors

        public LayoutModule(IUnityContainer container, IRegionManager regionManager)
        {
            this.Container = container;
            this.RegionManager = regionManager;
        }

        #endregion

        #region Properties

        public IUnityContainer Container { get; private set; }

        public IRegionManager RegionManager { get; private set; }

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingLayoutModule);

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Loading module ...");
            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Register<LayoutView, LayoutViewModel>();
            navigationService.Register<MenuView, MenuViewModel>();
            navigationService.Register<LoginView, LoginViewModel>();

            var notificationService = ServiceLocator.Current.GetInstance<INotificationServiceClient>();
            notificationService.StartAsync().ConfigureAwait(true);
            var inputService = ServiceLocator.Current.GetInstance<IInputService>();
            inputService.Start();
            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MAINCONTENT}",
                typeof(LayoutView));
            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MENU}", typeof(MenuView));

            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Method intentionally left empty.
        }

        #endregion
    }
}
