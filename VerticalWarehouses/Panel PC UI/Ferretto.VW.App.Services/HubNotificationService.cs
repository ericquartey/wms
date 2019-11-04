using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;
using IInstallationHubClient = Ferretto.VW.MAS.AutomationService.Contracts.Hubs.IInstallationHubClient;
using MessageNotifiedEventArgs = Ferretto.VW.MAS.AutomationService.Contracts.Hubs.MessageNotifiedEventArgs;

namespace Ferretto.VW.App.Services
{
    internal class HubNotificationService : IHubNotificationService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInstallationHubClient installationHubClient;

        private readonly Logger logger;

        #endregion

        #region Constructors

        public HubNotificationService(
            IEventAggregator eventAggregator,
            IInstallationHubClient installationHubClient)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));

            this.installationHubClient = installationHubClient ?? throw new System.ArgumentNullException(nameof(installationHubClient));
            this.installationHubClient.MessageReceived += this.OnMessageReceived;
            this.installationHubClient.MachineModeChanged += this.OnMachineModeChanged;
            this.installationHubClient.MachinePowerChanged += this.OnMachinePowerChanged;

            this.logger = LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Methods

        private void OnMachineModeChanged(object sender, MachineModeChangedEventArgs e)
        {
            this.eventAggregator
                .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                .Publish(e);
        }

        private void OnMachinePowerChanged(object sender, MachinePowerChangedEventArgs e)
        {
            this.eventAggregator
                .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                .Publish(e);
        }

        private void OnMessageReceived(object sender, MessageNotifiedEventArgs e)
        {
            switch (e.NotificationMessage)
            {
                case NotificationMessageUI<SensorsChangedMessageData> sv:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                        .Publish(sv);
                    break;

                case NotificationMessageUI<CalibrateAxisMessageData> cc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<CalibrateAxisMessageData>>()
                        .Publish(cc);
                    break;

                case NotificationMessageUI<SwitchAxisMessageData> sw:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<SwitchAxisMessageData>>()
                        .Publish(sw);
                    break;

                case NotificationMessageUI<ShutterPositioningMessageData> sp:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                        .Publish(sp);
                    break;

                case NotificationMessageUI<HomingMessageData> h:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<HomingMessageData>>()
                        .Publish(h);
                    break;

                case NotificationMessageUI<CurrentPositionMessageData> cp:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<CurrentPositionMessageData>>()
                        .Publish(cp);
                    break;

                case NotificationMessageUI<InverterExceptionMessageData> ie:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<InverterExceptionMessageData>>()
                        .Publish(ie);
                    break;

                case NotificationMessageUI<PositioningMessageData> vp:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<PositioningMessageData>>()
                        .Publish(vp);
                    break;

                case NotificationMessageUI<ResolutionCalibrationMessageData> rc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<ResolutionCalibrationMessageData>>()
                        .Publish(rc);
                    break;

                case NotificationMessageUI<ResetSecurityMessageData> sc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<ResetSecurityMessageData>>()
                        .Publish(sc);
                    break;

                case NotificationMessageUI<InverterStopMessageData> sc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<InverterStopMessageData>>()
                        .Publish(sc);
                    break;

                case NotificationMessageUI<PowerEnableMessageData> sc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<PowerEnableMessageData>>()
                        .Publish(sc);
                    break;

                case NotificationMessageUI<InverterStatusWordMessageData> isw:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<InverterStatusWordMessageData>>()
                        .Publish(isw);
                    break;

                case NotificationMessageUI<MachineStatusActiveMessageData> msa:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>()
                        .Publish(msa);
                    break;

                case NotificationMessageUI<MachineStateActiveMessageData> msa:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<MachineStateActiveMessageData>>()
                        .Publish(msa);
                    break;

                case NotificationMessageUI<ChangeRunningStateMessageData> crm:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<ChangeRunningStateMessageData>>()
                        .Publish(crm);
                    break;

                case NotificationMessageUI<MoveLoadingUnitMessageData> mld:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                        .Publish(mld);
                    break;

                default:
                    this.logger.Debug($"Signal-R hub message {e.NotificationMessage.GetType().Name} was ignored.");
                    break;
            }
        }

        #endregion
    }
}
