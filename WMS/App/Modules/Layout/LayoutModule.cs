using CommonServiceLocator;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.WMS.Modules.Layout
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class associate all Views to related ViewModels")]
    [Module(ModuleName = nameof(Common.Utils.Modules.Layout), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class LayoutModule : IModule
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

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
            var notificationService = ServiceLocator.Current.GetInstance<INotificationService>();
            notificationService.StartAsync().ConfigureAwait(true);

            var inputService = ServiceLocator.Current.GetInstance<IInputService>();
            inputService.Start();

            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MAINCONTENT}",
                typeof(LayoutView));

            this.RegionManager.RegisterViewWithRegion(
                $"{nameof(Common.Utils.Modules.Layout)}.{Common.Utils.Modules.Layout.REGION_MENU}",
                typeof(MenuView));

            this.logger.Trace("Module loaded.");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingLayoutModule);

            this.logger.Trace("Loading module ...");

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Register<LayoutView, LayoutViewModel>();
            navigationService.Register<MenuView, MenuViewModel>();
            navigationService.Register<LoginView, LoginViewModel>();
        }

        #endregion
    }
}
