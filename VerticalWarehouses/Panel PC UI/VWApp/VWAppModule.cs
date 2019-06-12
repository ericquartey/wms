using System.Configuration;
using Ferretto.VW.VWApp.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Unity;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;

namespace Ferretto.VW.VWApp
{
    public class VWAppModule : IModule
    {
        #region Fields

        private const string WmsServiceAddress = "WMSServiceAddress";

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public VWAppModule(IUnityContainer container)
        {
            this.container = container;
            var eventAggregator = new EventAggregator();
            this.container.RegisterInstance<IEventAggregator>(eventAggregator);
            var notificationCatcher = new NotificationCatcher(eventAggregator, container);
            this.container.RegisterInstance<INotificationCatcher>(notificationCatcher);
            var wmsServiceAddress = ConfigurationManager.AppSettings.Get(WmsServiceAddress);
            var itemsDataService = DataServiceFactory.GetService<IItemsDataService>(new System.Uri(wmsServiceAddress));
            this.container.RegisterInstance<IItemsDataService>(itemsDataService);
            var loadingUnitsDataService = DataServiceFactory.GetService<ILoadingUnitsDataService>(new System.Uri(wmsServiceAddress));
            this.container.RegisterInstance<ILoadingUnitsDataService>(loadingUnitsDataService);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // HACK IModule interface requires the implementation of this method
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // HACK IModule interface requires the implementation of this method
        }

        #endregion
    }
}
