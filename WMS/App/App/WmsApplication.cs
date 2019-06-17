using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommonServiceLocator;
using DevExpress.Utils.IoC;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Prism.Unity.Ioc;
using Prism.Unity.Regions;

namespace Ferretto.WMS.App
{
    public class WmsApplication : PrismApplicationBase
    {
        #region Methods

        public static void RegisterTypes(IContainerRegistry containerRegistry, IContainerProvider container)
        {
            ConfigureSignalRHub(containerRegistry);

            var navigationService = container.Resolve<NavigationService>();
            containerRegistry.RegisterInstance<INavigationService>(navigationService);
            var eventService = container.Resolve<EventService>();
            containerRegistry.RegisterInstance<IEventService>(eventService);
            var inputService = container.Resolve<InputService>();
            containerRegistry.RegisterInstance<IInputService>(inputService);
            var dialogService = container.Resolve<DialogService>();
            containerRegistry.RegisterInstance<IDialogService>(dialogService);
            var histViewService = container.Resolve<HistoryViewService>();
            containerRegistry.RegisterInstance<IHistoryViewService>(histViewService);
            var notificationService = container.Resolve<NotificationService>();
            containerRegistry.RegisterInstance<INotificationService>(notificationService);
            var statusBarViewModel = container.Resolve<StatusBarViewModel>();
            containerRegistry.RegisterInstance(statusBarViewModel);
        }

        public static void RegisterAdapterMappings(RegionAdapterMappings regionAdapterMappings, IContainerProvider container)
        {
            if (regionAdapterMappings != null)
            {
                regionAdapterMappings.RegisterMapping(typeof(Selector), container.Resolve<SelectorRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ItemsControl), container.Resolve<ItemsControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ContentControl), container.Resolve<ContentControlRegionAdapter>());
                var factory = ServiceLocator.Current.GetInstance<IRegionBehaviorFactory>();
                regionAdapterMappings.RegisterMapping(
                    typeof(LayoutPanel),
                    AdapterFactory.Make<RegionAdapterBase<LayoutPanel>>(factory));
            }
        }

        public static void LoadCatalog(IModuleCatalog moduleCatalog)
        {
            (moduleCatalog as ModuleCatalog)?.Load();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.ConfiguringPrismModuleCatalog);
            base.ConfigureModuleCatalog(moduleCatalog);
            LoadCatalog(moduleCatalog);
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            RegisterAdapterMappings(regionAdapterMappings, this.Container);
        }

        protected override Window CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            return new UnityContainerExtension();
        }

        protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterRequiredTypes(containerRegistry);
            containerRegistry.RegisterSingleton<IRegionNavigationContentLoader, UnityRegionNavigationContentLoader>();
            containerRegistry.RegisterSingleton<IServiceLocator, UnityServiceLocatorAdapter>();
        }

        protected override void RegisterFrameworkExceptionTypes()
        {
            base.RegisterFrameworkExceptionTypes();
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ResolutionFailedException));
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterTypes(containerRegistry, this.Container);
        }

        private static void ConfigureSignalRHub(IContainerRegistry containerRegistry)
        {
            const string dataServiceUrlConfigKey = "DataService:Url";
            const string dataServiceHubPathConfigKey = "DataService:HubPath";

            var dataServiceUrl = ConfigurationManager.AppSettings[dataServiceUrlConfigKey];
            var dataServiceHubPath = ConfigurationManager.AppSettings[dataServiceHubPathConfigKey];
            if (string.IsNullOrWhiteSpace(dataServiceUrl) || string.IsNullOrWhiteSpace(dataServiceHubPath))
            {
                throw new ConfigurationErrorsException(
                    $"Application settings keys '{dataServiceUrlConfigKey}' and '{dataServiceHubPathConfigKey}' are not properly configured.");
            }

            var baseUrl = new Uri(dataServiceUrl);
            var dataHubService = DataServiceFactory.GetService<IDataHubClient>(new Uri(baseUrl, dataServiceHubPath));
            containerRegistry.RegisterInstance(dataHubService);
            dataHubService.ConnectAsync();
        }

        #endregion
    }
}
