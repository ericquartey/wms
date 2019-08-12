using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
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

            containerProvider.Resolve<IHealthProbeService>().Start();

            var viewModel = (containerProvider.Resolve<IMainWindow>() as System.Windows.Window).DataContext;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<INotificationService, NotificationService>();
        }

        #endregion
    }
}
