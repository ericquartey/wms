using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
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
            if (receivedMessage.Status == MessageStatus.OperationEnd)
            {
                this.machineProvider.IsHomingExecuted = true;
            }
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

        private async Task OnAssignedMissionOperationChanged(AssignedMissionOperationChangedMessageData e)
        {
            Contract.Requires(e != null);

            await this.operatorHub.Clients.All.AssignedMissionOperationChanged(
                e.BayNumber,
                e.MissionId,
                e.MissionOperationId,
                e.PendingMissionsCount);
        }

        private void OnBayChainPositionChanged(BayChainPositionMessageData data)
        {
            Contract.Requires(data != null);

            this.installationHub.Clients.All.BayChainPositionChanged(data.Position, data.BayNumber);
        }

        private void OnBayConnected(BayOperationalStatusChangedMessageData data)
        {
            Contract.Requires(data != null);

            this.operatorHub.Clients.All.BayStatusChanged(data.BayNumber, data.BayStatus);
        }

        private void OnChangeRunningState(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is ChangeRunningStateMessageData data)
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

                    case MessageStatus.OperationError:
                    case MessageStatus.OperationStop:
                        machinePowerState = MachinePowerState.Unpowered;
                        break;

                    default:
                        machinePowerState = data.Enable ? MachinePowerState.Unpowered : MachinePowerState.Powered;
                        break;
                }
                this.machineProvider.IsMachineRunning = (machinePowerState == MachinePowerState.Powered);

                this.installationHub.Clients.All.MachinePowerChanged(machinePowerState);
            }
        }

        private void OnDataLayerReady()
        {
            this.baysDataProvider.AddElevatorPseudoBay();

            this.baysDataProvider.GetAll().ToList(); // HACK why is this call needed?
        }

        private void OnElevatorPositionChanged(ElevatorPositionMessageData data)
        {
            Contract.Requires(data != null);

            this.installationHub.Clients.All.ElevatorPositionChanged(
                data.VerticalPosition,
                data.HorizontalPosition,
                data.CellId,
                data.BayPositionId);
        }

        private void OnErrorStatusChanged(IErrorStatusMessageData machineErrorMessageData)
        {
            Contract.Requires(machineErrorMessageData != null);

            this.operatorHub.Clients.All.ErrorStatusChanged(machineErrorMessageData.ErrorId);
        }

        private void OnFsmException(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is FsmExceptionMessageData)
            {
                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.FsmException(messageToUi);
            }
        }

        private void OnInverterStatusWordChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.InverterStatusWordChanged(message);
        }

        private void OnMachineModeChanged(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is MachineModeMessageData data)
            {
                this.installationHub.Clients.All.MachineModeChanged(data.MachineMode);
            }
        }

        private void OnMoveLoadingUnit(NotificationMessage receivedMessage)
        {
            var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            this.installationHub.Clients.All.MoveLoadingUnit(messageToUi);
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
