using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.VWApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Events;

namespace Ferretto.VW.VWApp
{
    public class NotificationCatcher : INotificationCatcher
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public NotificationCatcher(IEventAggregator eventAggregator, IUnityContainer container)
        {
            this.eventAggregator = eventAggregator;
            this.container = container;
        }

        #endregion

        #region Methods

        public void SubscribeInstallationMethodsToMAService()
        {
            var installationHubClient = this.container.Resolve<IContainerInstallationHubClient>();

            installationHubClient.MessageNotified += this.MessageNotifiedEventHandler;
        }

        /// <summary>
        /// Delegate when an incoming Notification Message is catch from SignalR controller and the related event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageNotifiedEventHandler(object sender, MessageNotifiedEventArgs e)
        {
            if (e.NotificationMessage is NotificationMessageUI<SensorsChangedMessageData> ev)
            {
                var dataSensors = ev.Data.SensorsStates;

                this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Publish(ev);
            }
            if (e.NotificationMessage is NotificationMessageUI<CalibrateAxisMessageData> cc)
            {
                var data = cc.Data;
                var dataDescription = cc.Description;
                var status = cc.Status;

                this.eventAggregator.GetEvent<NotificationEventUI<CalibrateAxisMessageData>>().Publish(cc);
            }
            if (e.NotificationMessage is NotificationMessageUI<SwitchAxisMessageData> sw)
            {
                var data = sw.Data;

                this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>().Publish(sw);
            }

            // -
            // Adds other Notification events and publish it in the EventAggregator
            // -
        }

        #endregion
    }
}
