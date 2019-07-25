using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInstallationHubClient installationHubClient;

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public NotificationService(
            IEventAggregator eventAggregator,
            IOperatorHubClient operatorHubClient,
            IInstallationHubClient installationHubClient)
        {
            if (operatorHubClient == null)
            {
                throw new System.ArgumentNullException(nameof(operatorHubClient));
            }

            if (installationHubClient == null)
            {
                throw new System.ArgumentNullException(nameof(installationHubClient));
            }

            this.eventAggregator = eventAggregator;
            this.operatorHubClient = operatorHubClient;
            this.installationHubClient = installationHubClient;

            this.operatorHubClient.MessageNotified += this.OnMessageNotified;
            this.installationHubClient.MessageNotified += this.InstallationMessageNotifiedEventHandler;
        }

        #endregion

        #region Methods

        private void HandlePositioningMessageData(NotificationMessageUI<PositioningMessageData> vp)
        {
            this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Publish(vp);

            if (vp.Status == MessageStatus.OperationError
                &&
                vp.Data is PositioningMessageData positioningData)
            {
                var actionType = ActionType.None;
                switch (positioningData.AxisMovement)
                {
                    case Axis.Both:
                        actionType = ActionType.Homing;
                        break;

                    case Axis.Vertical:
                        actionType = ActionType.VerticalHoming;
                        break;

                    case Axis.Horizontal:
                        actionType = ActionType.HorizontalHoming;
                        break;

                    case Axis.None:
                        break;
                }

                this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                    new MAS_EventMessage(NotificationType.Error, actionType, ActionStatus.Error));
            }
        }

        private void HandleSensorsChangedMessage(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            var dataSensors = message.Data.SensorsStates;

            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Publish(message);

            if (!dataSensors[(int)IOMachineSensors.NormalState])
            {
                this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                    new MAS_EventMessage(NotificationType.Error, ActionType.SensorsChanged, ActionStatus.Error));
            }
        }

        /// <summary>
        /// Delegate when an incoming Notification Message is catch from SignalR controller and the related event is fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallationMessageNotifiedEventHandler(object sender, MessageNotifiedEventArgs e)
        {
            switch (e.NotificationMessage)
            {
                case NotificationMessageUI<SensorsChangedMessageData> sv:
                    this.HandleSensorsChangedMessage(sv);
                    break;

                case NotificationMessageUI<CalibrateAxisMessageData> cc:
                    this.eventAggregator.GetEvent<NotificationEventUI<CalibrateAxisMessageData>>().Publish(cc);

                    if (cc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                            new MAS_EventMessage(NotificationType.Error, ActionType.Homing, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<SwitchAxisMessageData> sw:
                    this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>().Publish(sw);

                    if (sw.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                            new MAS_EventMessage(NotificationType.Error, ActionType.SwitchAxis, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<ShutterPositioningMessageData> sp:
                    this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Publish(sp);

                    if (sp.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                            new MAS_EventMessage(NotificationType.Error, ActionType.ShutterPositioning, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<ShutterControlMessageData> sc:
                    this.eventAggregator.GetEvent<NotificationEventUI<ShutterControlMessageData>>().Publish(sc);

                    if (sc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                            new MAS_EventMessage(NotificationType.Error, ActionType.ShutterControl, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<HomingMessageData> h:
                    this.eventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Publish(h);

                    if (h.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                            new MAS_EventMessage(NotificationType.Error, ActionType.Homing, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<CurrentPositionMessageData> cp:
                    this.eventAggregator.GetEvent<NotificationEventUI<CurrentPositionMessageData>>().Publish(cp);
                    break;

                case NotificationMessageUI<InverterExceptionMessageData> ie:
                    this.eventAggregator.GetEvent<NotificationEventUI<InverterExceptionMessageData>>().Publish(ie);
                    break;

                case NotificationMessageUI<PositioningMessageData> vp:
                    this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Publish(vp);
                    this.HandlePositioningMessageData(vp);
                    break;

                case NotificationMessageUI<ResolutionCalibrationMessageData> rc:
                    this.eventAggregator.GetEvent<NotificationEventUI<ResolutionCalibrationMessageData>>().Publish(rc);

                    if (rc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MAS_ErrorEvent>().Publish(
                            new MAS_EventMessage(NotificationType.Error, ActionType.ResolutionCalibration, ActionStatus.Error));
                    }
                    break;
            }
        }

        private void OnMessageNotified(object sender, MessageNotifiedEventArgs e)
        {
            if (e.NotificationMessage is NotificationMessageUI<ExecuteMissionMessageData> dop)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<ExecuteMissionMessageData>>().Publish(dop);
            }

            if (e.NotificationMessage is NotificationMessageUI<BayConnectedMessageData> bayMessage)
            {
                this.eventAggregator.GetEvent<NotificationEventUI<BayConnectedMessageData>>().Publish(bayMessage);
            }
        }

        #endregion
    }
}
