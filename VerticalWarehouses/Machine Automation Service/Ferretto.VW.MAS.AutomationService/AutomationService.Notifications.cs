using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination == MessageActor.AutomationService
                ||
                notification.Destination == MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            switch (receivedMessage.Type)
            {
                case MessageType.SensorsChanged:
                    this.OnSensorsChanged(receivedMessage);
                    break;

                case MessageType.DlException:
                    this.OnDataLayerException(receivedMessage);
                    break;

                case MessageType.Homing:
                    this.HomingMethod(receivedMessage);
                    break;

                case MessageType.SwitchAxis:
                    this.SwitchAxisMethod(receivedMessage);
                    break;

                case MessageType.ShutterPositioning:
                    this.ShutterPositioningMethod(receivedMessage);
                    break;

                case MessageType.CalibrateAxis:
                    this.CalibrateAxisMethod(receivedMessage);
                    break;

                case MessageType.CurrentPosition:
                    this.CurrentPositionMethod(receivedMessage);
                    break;

                case MessageType.Positioning:
                    this.OnPositioningChanged(receivedMessage);
                    break;

                case MessageType.ResolutionCalibration:
                    this.ResolutionCalibrationMethod(receivedMessage);
                    break;

                case MessageType.ExecuteMission:
                    await this.OnNewMissionOperationAvailable(receivedMessage.Data as INewMissionOperationAvailable);
                    break;

                case MessageType.ElevatorWeightCheck:
                    this.ElevatorWeightCheckMethod(receivedMessage);
                    break;

                case MessageType.BayOperationalStatusChanged:
                    this.OnBayConnected(receivedMessage.Data as IBayOperationalStatusChangedMessageData);
                    break;

                case MessageType.ErrorStatusChanged:
                    this.OnErrorStatusChanged(receivedMessage.Data as IErrorStatusMessageData);
                    break;

                case MessageType.InverterStatusWord:
                    this.OnInverterStatusWordChanged(receivedMessage);
                    break;

                case MessageType.MachineStateActive:
                    this.MachineStateActiveMethod(receivedMessage);
                    break;

                case MessageType.MachineStatusActive:
                    this.MachineStatusActiveMethod(receivedMessage);
                    break;

                case MessageType.DataLayerReady:
                    this.OnDataLayerReady();
                    break;

                case MessageType.ChangeRunningState:
                    this.OnChangeRunningState(receivedMessage);
                    break;

                case MessageType.MoveLoadingUnit:
                    this.OnMoveLoadingUnit(receivedMessage);

                    break;
            }
        }

        #endregion
    }
}
