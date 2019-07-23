using System.Configuration;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.WmsCommunication;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace Ferretto.VW.App
{
    public class VWAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public VWAppModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IOperatorHubClient>().ConnectAsync();
            containerProvider.Resolve<IInstallationHubClient>().ConnectAsync();
            containerProvider.Resolve<IDataHubClient>().ConnectAsync();

            var viewModel = (containerProvider.Resolve<IMainWindow>() as System.Windows.Window).DataContext;

            (viewModel as MainWindowViewModel)?.HACK_InitialiseHubOperator();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<INotificationService, NotificationService>();

            RegisterWmsProviders(containerRegistry);

            RegisterHubs(containerRegistry);
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

        private static void RegisterWmsProviders(IContainerRegistry containerRegistry)
        {
            var wmsServiceUrl = new System.Uri(ConfigurationManager.AppSettings.Get("WMSServiceAddress"));

            containerRegistry.RegisterInstance<IWmsDataProvider>(new WmsDataProvider(wmsServiceUrl));
            containerRegistry.RegisterInstance<IWmsImagesProvider>(new WmsImagesProvider(wmsServiceUrl));
        }

        #endregion
    }
}
