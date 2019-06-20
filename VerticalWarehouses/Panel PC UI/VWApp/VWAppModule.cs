using System.Configuration;
using Ferretto.VW.VWApp.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Unity;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Ferretto.VW.WmsCommunication;
using Ferretto.VW.WmsCommunication.Interfaces;

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
            var wmsServiceAddress = ConfigurationManager.AppSettings.Get(WmsServiceAddress);
            var wmsUri = new System.Uri(wmsServiceAddress);
            var eventAggregator = new EventAggregator();
            var notificationCatcher = new NotificationCatcher(eventAggregator, container);
            var wmsCommunication = new WmsDataProvider(this.container, wmsUri);

            this.container.RegisterInstance<IEventAggregator>(eventAggregator);
            this.container.RegisterInstance<INotificationCatcher>(notificationCatcher);
            this.container.RegisterInstance<IWmsDataProvider>(wmsCommunication);
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
