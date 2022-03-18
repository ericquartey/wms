using System;
using System.Configuration;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class HubNotificationService : IHubNotificationService
    {
        #region Fields

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly IInstallationHubClient installationHubClient;

        private readonly Logger logger;

        private readonly IOperatorHubClient operatorHubClient;

        #endregion

        #region Constructors

        public HubNotificationService(
            IEventAggregator eventAggregator,
            IInstallationHubClient installationHubClient,
            IOperatorHubClient operatorHubClient)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.installationHubClient = installationHubClient ?? throw new ArgumentNullException(nameof(installationHubClient));
            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));

            this.installationHubClient.MessageReceived += this.OnMessageReceived;
            this.installationHubClient.MachineModeChanged += this.OnEventReceived;
            this.installationHubClient.MachinePowerChanged += this.OnEventReceived;
            this.installationHubClient.ElevatorPositionChanged += this.OnEventReceived;
            this.installationHubClient.BayChainPositionChanged += this.OnBayEventReceived;
            this.installationHubClient.SystemTimeChanged += this.OnEventReceived;

            this.operatorHubClient.ProductsChanged += this.OnEventReceived;

            this.logger = LogManager.GetCurrentClassLogger();

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Methods

        private void OnBayEventReceived<TEventArgs>(object sender, TEventArgs e)
            where TEventArgs : EventArgs, IBayEventArgs
        {
            if (e.BayNumber == this.bayNumber)
            {
                this.eventAggregator
                    .GetEvent<PubSubEvent<TEventArgs>>()
                    .Publish(e);
            }
        }

        private void OnEventReceived<TEventArgs>(object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            this.eventAggregator
                .GetEvent<PubSubEvent<TEventArgs>>()
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

                case NotificationMessageUI<FsmExceptionMessageData> fsm:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<FsmExceptionMessageData>>()
                        .Publish(fsm);
                    break;

                case NotificationMessageUI<InverterReadingMessageData> ir:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<InverterReadingMessageData>>()
                        .Publish(ir);
                    break;

                case NotificationMessageUI<InverterParametersMessageData> ipmd:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<InverterParametersMessageData>>()
                        .Publish(ipmd);
                    break;

                case NotificationMessageUI<ProfileCalibrationMessageData> pcmd:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>()
                        .Publish(pcmd);
                    break;

                case NotificationMessageUI<MoveTestMessageData> rc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<MoveTestMessageData>>()
                        .Publish(rc);
                    break;

                case NotificationMessageUI<RepetitiveHorizontalMovementsMessageData> rh:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<RepetitiveHorizontalMovementsMessageData>>()
                        .Publish(rh);
                    break;

                case NotificationMessageUI<InverterProgrammingMessageData> ip:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<InverterProgrammingMessageData>>()
                        .Publish(ip);
                    break;

                case NotificationMessageUI<CombinedMovementsMessageData> cm:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<CombinedMovementsMessageData>>()
                        .Publish(cm);
                    break;

                case NotificationMessageUI<SocketLinkAlphaNumericBarChangeMessageData> sla:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<SocketLinkAlphaNumericBarChangeMessageData>>()
                        .Publish(sla);
                    break;

                case NotificationMessageUI<SocketLinkLaserPointerChangeMessageData> sll:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<SocketLinkLaserPointerChangeMessageData>>()
                        .Publish(sll);
                    break;

                case NotificationMessageUI<SocketLinkOperationChangeMessageData> slo:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<SocketLinkOperationChangeMessageData>>()
                        .Publish(slo);
                    break;

                case NotificationMessageUI<LogoutMessageData> l:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<LogoutMessageData>>()
                        .Publish(l);
                    break;

                case NotificationMessageUI<DiagOutChangedMessageData> doc:
                    this.eventAggregator
                        .GetEvent<NotificationEventUI<DiagOutChangedMessageData>>()
                        .Publish(doc);
                    break;

                default:
                    this.logger.Debug($"Signal-R hub message {e.NotificationMessage.GetType().Name} was ignored.");
                    break;
            }
        }

        #endregion
    }
}
