using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.App.Services.Interfaces;
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
        }

        #endregion
    }
}
