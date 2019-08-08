using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.App
{
    internal class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        public void BindViewModelToView<TViewModel, TView>()
        {
            ViewModelLocationProvider.Register(typeof(TView).ToString(), () => this.Container.Resolve<TViewModel>());
        }

        protected override void ConfigureContainer()
        {
            var serviceUrl = ConfigurationManager.AppSettings.GetAutomationServiceUrl();
            var serviceLiveHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceLiveHealthPath();
            var serviceReadyHealthPath = ConfigurationManager.AppSettings.GetAutomationServiceReadyHealthPath();

            // UI services
            this.Container.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            this.Container.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            this.Container.RegisterSingleton<IBayManager, BayManager>();
            this.Container.RegisterSingleton<IHealthProbeService>(
                new InjectionFactory(c =>
                    new HealthProbeService(serviceUrl, serviceLiveHealthPath, serviceReadyHealthPath, c.Resolve<IEventAggregator>())));
            this.Container.RegisterInstance(ServiceFactory.Get<IThemeService>());
            this.Container.RegisterInstance(ServiceFactory.Get<ISessionService>());

            // MAS Web API services
            var operatorHubPath = ConfigurationManager.AppSettings.GetAutomationServiceOperatorHubPath();
            var installationHubPath = ConfigurationManager.AppSettings.GetAutomationServiceInstallationHubPath();
            this.Container.RegisterMachineAutomationServices(serviceUrl);
            this.Container.RegisterMachineAutomationHubs(serviceUrl, operatorHubPath, installationHubPath);

            this.Container.RegisterType<MainWindowViewModel>();

            this.Container.RegisterSingleton<IMainWindow, MainWindow>();

            RegisterHubs(this.Container);
            RegisterWmsProviders(this.Container);

            base.ConfigureContainer();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.Resolve<IMainWindow>() as DependencyObject;
        }

        protected override void InitializeShell()
        {
            var mainWindowViewModel = this.Container.Resolve<MainWindowViewModel>();
            mainWindowViewModel.InitializeViewModelAsync(this.Container);

            var application = Application.Current as App;
            application.MainWindow.DataContext = mainWindowViewModel;

            application.MainWindow.Show();
        }

        private static void RegisterHubs(IUnityContainer container)
        {
            var wmsHubPath = ConfigurationManager.AppSettings.Get("WMS:DataService:Hubs:Data:Path");
            var wmsHub = DataServiceFactory.GetService<IDataHubClient>(new System.Uri(wmsHubPath));
            container.RegisterInstance(wmsHub);
        }

        private static void RegisterWmsProviders(IUnityContainer container)
        {
            var wmsServiceUrl = new System.Uri(ConfigurationManager.AppSettings.Get("WMS:DataService:Url"));

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

        #endregion
    }
}
