using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;

namespace Ferretto.VW.App
{
    public partial class App : PrismApplication, IInstallation
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public App()
        {
            System.AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        }

        #endregion

        #region Properties

        public Window InstallationAppMainWindowInstance { get; set; }

        public BindableBase InstallationAppMainWindowViewModel { get; set; }

        #endregion

        #region Methods

        public static void LoadCatalog(IModuleCatalog moduleCatalog)
        {
            (moduleCatalog as ModuleCatalog)?.Load();
        }

        public void ShowInstallation()
        {
            this.InstallationAppMainWindowInstance?.Show();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            LoadCatalog(moduleCatalog);
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = $"{MvvmNaming.GetViewModelName(viewName)}, {viewAssemblyName}";
                return Type.GetType(viewModelName);
            });
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override Window CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            this.HACK_ForceItalianLanguage();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

            var navigationService = this.Container.Resolve<NavigationService>();
            containerRegistry.RegisterInstance<INavigationService>(navigationService);

            containerRegistry.RegisterInstance<IUsersService>(new UsersService(automationServiceUrl));

            // UI services
            containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            containerRegistry.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            containerRegistry.RegisterSingleton<IBayManager, BayManager>();

            containerRegistry.RegisterInstance(ServiceFactory.Get<IThemeService>());
            containerRegistry.RegisterInstance(ServiceFactory.Get<ISessionService>());

            // MAS Web API services
            containerRegistry.RegisterInstance<IIdentityService>(new IdentityService(automationServiceUrl));
            containerRegistry.RegisterInstance<IMissionOperationsService>(new MissionOperationsService(automationServiceUrl));

            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<INotificationService, NotificationService>();

            RegisterHubs(containerRegistry);

            RegisterWmsProviders(containerRegistry);
        }

        private static void RegisterHubs(IContainerRegistry containerRegistry)
        {
            var automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

            var operatorHubPath = ConfigurationManager.AppSettings.Get("OperatorHubEndpoint");
            var operatorHubUrl = new System.Uri(new System.Uri(automationServiceUrl), operatorHubPath);
            var operatorHubClient = new OperatorHubClient(operatorHubUrl);
            containerRegistry.RegisterInstance<IOperatorHubClient>(operatorHubClient);

            var installationHubPath = ConfigurationManager.AppSettings.Get("InstallationHubEndpoint");
            var installationHubClient = new InstallationHubClient(automationServiceUrl, installationHubPath);
            containerRegistry.RegisterInstance<IInstallationHubClient>(installationHubClient);

            var wmsHubPath = ConfigurationManager.AppSettings.Get("WMSServiceAddressHubsEndpoint");
            var wmsHub = DataServiceFactory.GetService<IDataHubClient>(new System.Uri(wmsHubPath));
            containerRegistry.RegisterInstance(wmsHub);
        }

        private static void RegisterWmsProviders(IContainerRegistry container)
        {
            var wmsServiceUrl = new System.Uri(ConfigurationManager.AppSettings.Get("WMSServiceAddress"));

            container.RegisterSingleton<IWmsDataProvider, WmsDataProvider>();
            container.RegisterSingleton<IWmsImagesProvider, WmsImagesProvider>();

            container.RegisterInstance(DataServiceFactory.GetService<IBaysDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IImagesDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionOperationsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemListsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IAreasDataService>(wmsServiceUrl));
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as System.Exception, "An unhandled exception was thrown.");
        }

        private void HACK_ForceItalianLanguage()
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
        }

        #endregion
    }
}
