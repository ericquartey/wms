using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.VW.VWApp.Interfaces;
using Unity;
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
            var installationHubClient = this.container.Resolve<IInstallationHubClient>();

            installationHubClient.MessageNotified += this.InstallationMessageNotifiedEventHandler;
        }

        public void SubscribeOperatorMethodsToMAService()
        {
            var operatorHubClient = this.container.Resolve<IOperatorHubClient>();
            operatorHubClient.MessageNotified += this.OperatorMessageNotifiedEventHandler;
        }

        /// <summary>
        /// Delegate when an incoming Notification Message is catch from SignalR controller and the related event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallationMessageNotifiedEventHandler(object sender, MessageNotifiedEventArgs e)
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

                if (cc.Status == MessageStatus.OperationError)
                {
                    this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(new MAS_EventMessage(NotificationType.Error, ActionType.Homing, ActionStatus.Error));
                }
            }
            if (e.NotificationMessage is NotificationMessageUI<SwitchAxisMessageData> sw)
            {
                var data = sw.Data;

                this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>().Publish(sw);
            }
            if (e.NotificationMessage is NotificationMessageUI<ShutterPositioningMessageData> sp)
            {
                var data = sp.Data;

                this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Publish(sp);
            }
            if (e.NotificationMessage is NotificationMessageUI<ShutterControlMessageData> sc)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<ShutterControlMessageData>>().Publish(sc);

                if (sc.Status == MessageStatus.OperationError)
                {
                    this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(new MAS_EventMessage(NotificationType.Error, ActionType.ShutterControl, ActionStatus.Error));
                }
            }

            if (e.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Publish(h);
            }

            if (e.NotificationMessage is NotificationMessageUI<CurrentPositionMessageData> cp)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<CurrentPositionMessageData>>().Publish(cp);
            }

            if (e.NotificationMessage is NotificationMessageUI<PositioningMessageData> vp)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Publish(vp);
            }

            if (e.NotificationMessage is NotificationMessageUI<ResolutionCalibrationMessageData> rc)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<ResolutionCalibrationMessageData>>().Publish(rc);
            }

            // -
            // Adds other Notification events and publish it in the EventAggregator
            // -
        }

        private void OperatorMessageNotifiedEventHandler(object sender, OperatorApp.ServiceUtilities.MessageNotifiedEventArgs e)
        {
            if (e.NotificationMessage is NotificationMessageUI<ExecuteMissionMessageData> dop)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<ExecuteMissionMessageData>>().Publish(dop);
            }
        }

        #endregion
    }
}
