using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            Contract.Requires(notification != null);

            return
                notification.Destination is MessageActor.WebApi
                ||
                notification.Destination is MessageActor.AutomationService
                ||
                notification.Destination is MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            Contract.Requires(message != null);

            if (message.ErrorLevel is ErrorLevel.Fatal)
            {
                this.Logger.LogCritical(message.Description);
                this.applicationLifetime.StopApplication();
            }

            switch (message.Type)
            {
                case MessageType.SensorsChanged:
                    await this.OnSensorsChanged(message);
                    break;

                case MessageType.DiagOutChanged:
                    await this.OnDiagOutChanged(message);
                    break;

                case MessageType.MachineMode:
                    await this.OnMachineModeChanged(message);
                    break;

                case MessageType.Homing:
                    await this.HomingMethod(message, serviceProvider);
                    break;

                case MessageType.SwitchAxis:
                    await this.SwitchAxisMethod(message);
                    break;

                case MessageType.ShutterPositioning:
                    await this.ShutterPositioningMethod(message);
                    break;

                case MessageType.CalibrateAxis:
                    await this.CalibrateAxisMethod(message);
                    break;

                case MessageType.ElevatorPosition when message.Data is ElevatorPositionMessageData:
                    await this.OnElevatorPositionChanged(message.Data as ElevatorPositionMessageData);
                    break;

                case MessageType.BayChainPosition when message.Data is BayChainPositionMessageData:
                    await this.OnBayChainPositionChanged(message.Data as BayChainPositionMessageData);
                    break;

                case MessageType.Positioning:
                    await this.OnPositioningChanged(message);
                    break;

                case MessageType.ResolutionCalibration:
                    await this.ResolutionCalibrationMethod(message);
                    break;

                case MessageType.AssignedMissionChanged when message.Data is AssignedMissionChangedMessageData:
                    await this.OnAssignedMissionOperationChanged(message.Data as AssignedMissionChangedMessageData);
                    break;

                case MessageType.ElevatorWeightCheck:
                    await this.ElevatorWeightCheckMethod(message);
                    break;

                case MessageType.BayOperationalStatusChanged when message.Data is BayOperationalStatusChangedMessageData:
                    await this.OnBayConnected(message.Data as BayOperationalStatusChangedMessageData);
                    break;

                case MessageType.ErrorStatusChanged when message.Data is IErrorStatusMessageData:
                    await this.OnErrorStatusChanged(message.Data as IErrorStatusMessageData, message.RequestingBay);
                    break;

                case MessageType.InverterStatusWord:
                    await this.OnInverterStatusWordChanged(message);
                    break;

                case MessageType.MachineStateActive:
                    await this.MachineStateActiveMethod(message);
                    break;

                case MessageType.MachineStatusActive:
                    await this.MachineStatusActiveMethod(message);
                    break;

                case MessageType.DataLayerReady:
                    await this.OnDataLayerReady(serviceProvider);
                    break;

                case MessageType.ChangeRunningState:
                    await this.OnChangeRunningState(message);
                    break;

                case MessageType.MoveLoadingUnit:
                    await this.OnMoveLoadingUnit(message);
                    break;

                case MessageType.FsmException:
                    await this.OnFsmException(message);
                    break;

                case MessageType.BayLight:
                    await this.OnBayLight(message);
                    break;

                case MessageType.ProfileCalibration:
                    await this.OnProfileCalibration(message);
                    break;

                case MessageType.MoveTest:
                    await this.OnMoveTest(message);
                    break;

                //case MessageType.SocketLinkEnableChanged:
                //    await this.OnSocketLinkEnableChanged(serviceProvider);
                //    break;

                case MessageType.SocketLinkAlphaNumericBarChange:
                    await this.OnSocketLinkAlphaNumericBarChange(message);
                    break;

                case MessageType.SocketLinkLaserPointerChange:
                    await this.OnSocketLinkLaserPointerChange(message);
                    break;

                case MessageType.SocketLinkOperationChange:
                    await this.OnSocketLinkOperationChange(message);
                    break;

                case MessageType.RepetitiveHorizontalMovements:
                    await this.OnRepetitiveHorizontalMovementsChanged(message);
                    break;

                case MessageType.InverterProgramming:
                    await this.OnInverterProgrammingChanged(message);
                    break;

                case MessageType.InverterReading:
                    await this.OnInverterReadingChanged(message);
                    break;

                case MessageType.InverterParameters:
                    await this.OnInverterParameterChanged(message);
                    break;

                case MessageType.Logout:
                    await this.OnLogoutChanged(message);
                    break;
            }
        }

        #endregion
    }
}
