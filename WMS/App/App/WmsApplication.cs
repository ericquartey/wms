using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommonServiceLocator;
using DevExpress.Utils.IoC;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Prism;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Prism.Unity.Ioc;
using Prism.Unity.Regions;
using Unity;

namespace Ferretto.WMS.App
{
#pragma warning disable S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)

    public class WmsApplication : PrismApplicationBase
#pragma warning restore S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)
    {
        #region Methods

        public static void RegisterTypes(IContainerRegistry containerRegistry, IContainerProvider container)
        {
            containerRegistry.Register<IEventService, EventService>();
            containerRegistry.Register<IImageProvider, ImageProvider>();
            var navigationService = container.Resolve<NavigationService>();
            containerRegistry.RegisterInstance<INavigationService>(navigationService);
            var inputService = container.Resolve<InputService>();
            containerRegistry.RegisterInstance<IInputService>(inputService);
            var dialogService = container.Resolve<DialogService>();
            containerRegistry.RegisterInstance<IDialogService>(dialogService);
            var histViewService = container.Resolve<HistoryViewService>();
            containerRegistry.RegisterInstance<IHistoryViewService>(histViewService);
            var notificationServiceClient = container.Resolve<NotificationServiceClient>();
            containerRegistry.RegisterInstance<INotificationServiceClient>(notificationServiceClient);
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

        protected virtual IUnityContainer CreateContainer()
        {
            return new UnityContainer();
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

        #endregion
    }
}
