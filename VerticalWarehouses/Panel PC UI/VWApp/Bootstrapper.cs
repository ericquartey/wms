using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

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
            var automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

            // UI services
            this.Container.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            this.Container.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            this.Container.RegisterSingleton<IBayManager, BayManager>();
            this.Container.RegisterInstance(ServiceFactory.Get<IThemeService>());
            this.Container.RegisterInstance(ServiceFactory.Get<ISessionService>());

            // MAS Web API services
            this.Container.RegisterInstance<IIdentityService>(new IdentityService(automationServiceUrl));
            this.Container.RegisterInstance<IMissionOperationsService>(new MissionOperationsService(automationServiceUrl));

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
            var automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

            var operatorHubPath = ConfigurationManager.AppSettings.Get("OperatorHubEndpoint");
            var operatorHubUrl = new System.Uri(new System.Uri(automationServiceUrl), operatorHubPath);
            var operatorHubClient = new OperatorHubClient(operatorHubUrl);
            container.RegisterInstance<IOperatorHubClient>(operatorHubClient);

            var installationHubPath = ConfigurationManager.AppSettings.Get("InstallationHubEndpoint");
            var installationHubClient = new InstallationHubClient(automationServiceUrl, installationHubPath);
            container.RegisterInstance<IInstallationHubClient>(installationHubClient);

            var wmsHubPath = ConfigurationManager.AppSettings.Get("WMSServiceAddressHubsEndpoint");
            var wmsHub = DataServiceFactory.GetService<IDataHubClient>(new System.Uri(wmsHubPath));
            container.RegisterInstance(wmsHub);
        }

        private static void RegisterWmsProviders(IUnityContainer container)
        {
            var wmsServiceUrl = new System.Uri(ConfigurationManager.AppSettings.Get("WMSServiceAddress"));

            container.RegisterSingleton<IWmsDataProvider, WmsDataProvider>();
            container.RegisterSingleton<IWmsImagesProvider, WmsImagesProvider>();

            container.RegisterInstance(DataServiceFactory.GetService<IUsersDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IBaysDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IImagesDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionOperationsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IMissionsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<ILoadingUnitsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemsDataService>(wmsServiceUrl));
            container.RegisterInstance(DataServiceFactory.GetService<IItemListsDataService>(wmsServiceUrl));
        }

        #endregion
    }
}
