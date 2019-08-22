using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Prism.Events;
using IInstallationHubClient = Ferretto.VW.MAS.AutomationService.Contracts.Hubs.IInstallationHubClient;
using MessageNotifiedEventArgs = Ferretto.VW.MAS.AutomationService.Contracts.Hubs.MessageNotifiedEventArgs;

namespace Ferretto.VW.App.Services
{
    internal class NotificationService : INotificationService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInstallationHubClient installationHubClient;

        #endregion

        #region Constructors

        public NotificationService(
            IEventAggregator eventAggregator,
            IInstallationHubClient installationHubClient)
        {
            if (installationHubClient is null)
            {
                throw new System.ArgumentNullException(nameof(installationHubClient));
            }

            this.eventAggregator = eventAggregator;
            this.installationHubClient = installationHubClient;
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

                this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                    new MachineAutomationEventArgs(NotificationType.Error, actionType, ActionStatus.Error));
            }
        }

        private void HandleSensorsChangedMessage(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            var dataSensors = message.Data.SensorsStates;

            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Publish(message);

            if (!dataSensors[(int)IOMachineSensors.NormalState])
            {
                this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                    new MachineAutomationEventArgs(NotificationType.Error, ActionType.SensorsChanged, ActionStatus.Error));
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
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.Homing, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<SwitchAxisMessageData> sw:
                    this.eventAggregator.GetEvent<NotificationEventUI<SwitchAxisMessageData>>().Publish(sw);

                    if (sw.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.SwitchAxis, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<ShutterPositioningMessageData> sp:
                    this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Publish(sp);

                    if (sp.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.ShutterPositioning, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<ShutterTestStatusChangedMessageData> sc:
                    this.eventAggregator.GetEvent<NotificationEventUI<ShutterTestStatusChangedMessageData>>().Publish(sc);

                    if (sc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.ShutterControl, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<HomingMessageData> h:
                    this.eventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Publish(h);

                    if (h.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.Homing, ActionStatus.Error));
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
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.ResolutionCalibration, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<ResetSecurityMessageData> sc:
                    this.eventAggregator.GetEvent<NotificationEventUI<ResetSecurityMessageData>>().Publish(sc);

                    if (sc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.ResetSecurity, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<InverterStopMessageData> sc:
                    this.eventAggregator.GetEvent<NotificationEventUI<InverterStopMessageData>>().Publish(sc);

                    if (sc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.InverterStop, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<PowerEnableMessageData> sc:
                    this.eventAggregator.GetEvent<NotificationEventUI<PowerEnableMessageData>>().Publish(sc);

                    if (sc.Status == MessageStatus.OperationError)
                    {
                        this.eventAggregator.GetEvent<MachineAutomationErrorPubSubEvent>().Publish(
                            new MachineAutomationEventArgs(NotificationType.Error, ActionType.PowerEnable, ActionStatus.Error));
                    }
                    break;

                case NotificationMessageUI<InverterStatusWordMessageData> isw:
                    this.eventAggregator.GetEvent<NotificationEventUI<InverterStatusWordMessageData>>().Publish(isw);
                    break;

                case NotificationMessageUI<MachineStatusActiveMessageData> msa:
                    this.eventAggregator.GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>().Publish(msa);
                    break;

                case NotificationMessageUI<MachineStateActiveMessageData> msa:
                    this.eventAggregator.GetEvent<NotificationEventUI<MachineStateActiveMessageData>>().Publish(msa);
                    break;
            }
        }

        #endregion
    }
}
