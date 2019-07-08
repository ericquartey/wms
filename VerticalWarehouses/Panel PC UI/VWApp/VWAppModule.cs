namespace Ferretto.VW.VWApp
{
    using System.Configuration;
    using Ferretto.VW.VWApp.Interfaces;
    using Ferretto.VW.WmsCommunication;
    using Ferretto.VW.WmsCommunication.Interfaces;
    using Prism.Events;
    using Prism.Ioc;
    using Prism.Modularity;
    using Unity;

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
            var wmsDataProvider = new WmsDataProvider(this.container, wmsUri);
            var wmsImagesProvider = new WmsImagesProvider(this.container, wmsUri);

            this.container.RegisterInstance<IEventAggregator>(eventAggregator);
            this.container.RegisterInstance<INotificationCatcher>(notificationCatcher);
            this.container.RegisterInstance<IWmsDataProvider>(wmsDataProvider);
            this.container.RegisterInstance<IWmsImagesProvider>(wmsImagesProvider);
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
