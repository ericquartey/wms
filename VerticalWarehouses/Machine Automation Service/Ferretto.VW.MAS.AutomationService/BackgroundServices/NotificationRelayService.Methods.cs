using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService
    {
        #region Methods

        private void CalibrateAxisMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.CalibrateAxisNotify(message);
        }

        private void CurrentPositionMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.CurrentPositionChanged(message);
        }

        private void ElevatorWeightCheckMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.ElevatorWeightCheck(message);

            this.Logger.LogTrace($"30:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
        }

        private void HomingMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.HomingProcedureStatusChanged(message);
        }

        private void MachineStateActiveMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.MachineStateActiveNotify(message);
        }

        private void MachineStatusActiveMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.MachineStatusActiveNotify(message);
        }

        private void OnBayConnected(IBayOperationalStatusChangedMessageData messageData)
        {
            if (messageData is null)
            {
                throw new ArgumentNullException(nameof(messageData));
            }

            this.operatorHub.Clients.All.BayStatusChanged(messageData);
        }

        private void OnChangeRunningState(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is CommonUtils.Messages.Data.ChangeRunningStateMessageData data)
            {
                MachinePowerState machinePowerState;
                switch (receivedMessage.Status)
                {
                    case MessageStatus.OperationStart:
                    case MessageStatus.OperationExecuting:
                        machinePowerState = data.Enable ? MachinePowerState.PoweringUp : MachinePowerState.PoweringDown;
                        break;

                    case MessageStatus.OperationEnd:
                        machinePowerState = data.Enable ? MachinePowerState.Powered : MachinePowerState.Unpowered;
                        break;

                    default:
                        machinePowerState = data.Enable ? MachinePowerState.Unpowered : MachinePowerState.Powered;
                        break;
                }

                this.installationHub.Clients.All.MachinePowerChanged(machinePowerState);
            }
        }

        private void OnDataLayerException(NotificationMessage receivedMessage)
        {
            if (receivedMessage.ErrorLevel is ErrorLevel.Critical)
            {
                this.Logger.LogCritical(receivedMessage.Description);
                this.applicationLifetime.StopApplication();
            }
        }

        private void OnDataLayerReady()
        {
            this.baysProvider.AddElevatorPseudoBay();

            this.baysProvider.GetAll().ToList(); // HACK why is this call needed?
        }

        private void OnErrorStatusChanged(IErrorStatusMessageData machineErrorMessageData)
        {
            if (machineErrorMessageData is null)
            {
                throw new ArgumentNullException(nameof(machineErrorMessageData));
            }

            this.operatorHub.Clients.All.ErrorStatusChanged(machineErrorMessageData.ErrorId);
        }

        private void OnInverterStatusWordChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.InverterStatusWordChanged(message);
        }

        private void OnMachineModeChanged(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is DataLayer.MachineModeMessageData data)
            {
                this.installationHub.Clients.All.MachineModeChanged(data.MachineMode);
            }
        }

        private void OnMoveLoadingUnit(NotificationMessage receivedMessage)
        {
            var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.MoveLoadingUnit(messageToUi);
        }

        private async Task OnNewMissionOperationAvailable(INewMissionOperationAvailable e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            await this.operatorHub.Clients.All.NewMissionOperationAvailable(e);
        }

        private void OnPositioningChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);

            this.installationHub.Clients.All.PositioningNotify(message);
        }

        private void OnSensorsChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.SensorsChanged(message);
        }

        private void ResolutionCalibrationMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.ResolutionCalibrationNotify(message);
        }

        private void ShutterPositioningMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.ShutterPositioningNotify(message);
        }

        private void SwitchAxisMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.SwitchAxisNotify(message);
        }

        #endregion
    }
}
