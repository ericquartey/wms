using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination is MessageActor.AutomationService
                ||
                notification.Destination is MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.SensorsChanged:
                    this.OnSensorsChanged(message);
                    break;

                case MessageType.MachineMode:
                    this.OnMachineModeChanged(message);
                    break;

                case MessageType.DlException:
                    this.OnDataLayerException(message);
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

                case MessageType.CurrentPosition:
                    this.CurrentPositionMethod(message);
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
