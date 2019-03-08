using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
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
            installationHubClient.SensorsChanged += this.RaiseSensorsChangedEvent;
            installationHubClient.ReceivedMessage += this.RaiseReceivedMessageEvent;
            installationHubClient.ActionUpdated += this.RaiseActionUpdatedEvent;
        }

        private void RaiseActionUpdatedEvent(object sender, IActionUpdateData data)
        {
            var messageData = new MAS_EventMessage(data.NotificationType, data.ActionType, data.ActionStatus);
            this.eventAggregator.GetEvent<MAS_Event>().Publish(messageData);
        }

        private void RaiseReceivedMessageEvent(object sender, string message)
        {
            var messageData = new NotificationMessageReceivedMessageData(message);
            this.eventAggregator.GetEvent<MAS_Event>().Publish(new MAS_EventMessage(NotificationType.Action, ActionType.None, ActionStatus.None, messageData));
        }

        private void RaiseSensorsChangedEvent(object sender, bool[] message)
        {
            var messageData = new NotificationMessageSensorsChangedData(message);
            this.eventAggregator.GetEvent<MAS_Event>().Publish(new MAS_EventMessage(NotificationType.SensorsChanged, ActionType.None, ActionStatus.None, messageData));
        }

        #endregion
    }
}
