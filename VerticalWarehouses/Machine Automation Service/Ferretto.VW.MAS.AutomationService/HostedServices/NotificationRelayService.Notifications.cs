using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            System.Diagnostics.Contracts.Contract.Requires(notification != null);

            return
                notification.Destination is MessageActor.AutomationService
                ||
                notification.Destination is MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            System.Diagnostics.Contracts.Contract.Requires(message != null);

            if (message.ErrorLevel is ErrorLevel.Fatal)
            {
                this.Logger.LogCritical(message.Description);
                this.applicationLifetime.StopApplication();
            }

            switch (message.Type)
            {
                case MessageType.SensorsChanged:
                    this.OnSensorsChanged(message);
                    break;

                case MessageType.MachineMode:
                    this.OnMachineModeChanged(message);
                    break;

                case MessageType.Homing:
                    this.HomingMethod(message);
                    break;

                case MessageType.SwitchAxis:
                    this.SwitchAxisMethod(message);
                    break;

                case MessageType.ShutterPositioning:
                    this.ShutterPositioningMethod(message);
                    break;

                case MessageType.CalibrateAxis:
                    this.CalibrateAxisMethod(message);
                    break;

                case MessageType.ElevatorPosition:
                    this.OnElevatorPositionChanged(message);
                    break;

                case MessageType.Positioning:
                    this.OnPositioningChanged(message);
                    break;

                case MessageType.ResolutionCalibration:
                    this.ResolutionCalibrationMethod(message);
                    break;

                case MessageType.ExecuteMission:
                    await this.OnNewMissionOperationAvailable(message.Data as INewMissionOperationAvailable);
                    break;

                case MessageType.ElevatorWeightCheck:
                    this.ElevatorWeightCheckMethod(message);
                    break;

                case MessageType.BayOperationalStatusChanged:
                    this.OnBayConnected(message.Data as IBayOperationalStatusChangedMessageData);
                    break;

                case MessageType.ErrorStatusChanged:
                    this.OnErrorStatusChanged(message.Data as IErrorStatusMessageData);
                    break;

                case MessageType.InverterStatusWord:
                    this.OnInverterStatusWordChanged(message);
                    break;

                case MessageType.MachineStateActive:
                    this.MachineStateActiveMethod(message);
                    break;

                case MessageType.MachineStatusActive:
                    this.MachineStatusActiveMethod(message);
                    break;

                case MessageType.DataLayerReady:
                    this.OnDataLayerReady();
                    break;

                case MessageType.ChangeRunningState:
                    this.OnChangeRunningState(message);
                    break;

                case MessageType.MoveLoadingUnit:
                    this.OnMoveLoadingUnit(message);
                    break;
            }
        }

        #endregion
    }
}
