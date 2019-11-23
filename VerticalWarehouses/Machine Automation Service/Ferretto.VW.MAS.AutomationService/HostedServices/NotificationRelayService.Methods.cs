using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
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

        private void OnBayChainPositionChanged(BayChainPositionMessageData data)
        {
            Contract.Requires(data != null);

            this.installationHub.Clients.All.BayChainPositionChanged(
                data.Position,
                data.BayNumber);
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
            this.baysProvider.AddElevatorPseudoBay();

            this.baysProvider.GetAll().ToList(); // HACK why is this call needed?
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

        private async Task OnNewMissionOperationAvailable(INewMissionOperationAvailable e)
        {
            Contract.Requires(e != null);

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
