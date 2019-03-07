using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.VWApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Events;

namespace Ferretto.VW.VWApp
{
    public class NotificationCatcher : INotificationCatcher
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

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
            var installationHubClient = this.container.Resolve<IContainerInstallationHubClient>() as InstallationHubClient;
            installationHubClient.SensorsChanged += this.RaiseSensorsChangedEvent;
            installationHubClient.ReceivedMessage += this.RaiseReceivedMessageEvent;
        }

        private void RaiseReceivedMessageEvent(object sender, string message)
        {
            this.eventAggregator.GetEvent<MAS_Event>().Publish(new MAS_EventMessage(NotificationType.Action, ActionType.None, ActionStatus.None, null));
        }

        private void RaiseSensorsChangedEvent(object sender, bool[] message)
        {
            var messageData = new NotificationMessageSensorsChangedData(message);
            this.eventAggregator.GetEvent<MAS_Event>().Publish(new MAS_EventMessage(NotificationType.SensorsChanged, ActionType.None, ActionStatus.None, messageData));
        }

        #endregion
    }
}
