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
    public partial class AutomationService
    {
        #region Methods

        private void CalibrateAxisMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.Logger.LogTrace($"13:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.CalibrateAxisNotify(messageToUi);

                this.Logger.LogTrace($"14:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"15:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"16:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void CurrentPositionMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.CurrentPositionChanged(message);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"5:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void ElevatorWeightCheckMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.Logger.LogTrace($"29:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ElevatorWeightCheck(message);

                this.Logger.LogTrace($"30:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"31:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"32:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void HomingMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.HomingProcedureStatusChanged(message);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"5:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void MachineStateActiveMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var msgUI = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.MachineStateActiveNotify(msgUI);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void MachineStatusActiveMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var msgUI = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.MachineStatusActiveNotify(msgUI);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnBayConnected(IBayOperationalStatusChangedMessageData messageData)
        {
            if (messageData == null)
            {
                throw new ArgumentNullException(nameof(messageData));
            }

            this.operatorHub.Clients.All.BayStatusChanged(messageData);
        }

        private void OnChangeRunningState(NotificationMessage receivedMessage)
        {
            try
            {
                if (receivedMessage.Data is CommonUtils.Messages.Data.ChangeRunningStateMessageData data)
                {
                    var machinePowerState = MachinePowerState.NotSpecified;

                    switch (receivedMessage.Status)
                    {
                        case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                        case CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting:
                            machinePowerState = data.Enable ? MachinePowerState.PoweringUp : MachinePowerState.PoweringDown;
                            break;

                        case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                            machinePowerState = data.Enable ? MachinePowerState.Powered : MachinePowerState.Unpowered;
                            break;

                        default:
                            machinePowerState = data.Enable ? MachinePowerState.Unpowered : MachinePowerState.Powered;
                            break;
                    }

                    this.installationHub.Clients.All.MachinePowerChanged(machinePowerState);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"8:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnDataLayerException(NotificationMessage receivedMessage)
        {
            if (receivedMessage.ErrorLevel == ErrorLevel.Critical)
            {
                this.Logger.LogCritical(receivedMessage.Description);
                this.applicationLifetime.StopApplication();
            }
        }

        private void OnDataLayerReady()
        {
            this.baysProvider.AddElevatorPseudoBay();

            this.baysProvider.GetAll().ToList();
        }

        private void OnErrorStatusChanged(IErrorStatusMessageData machineErrorMessageData)
        {
            if (machineErrorMessageData == null)
            {
                throw new ArgumentNullException(nameof(machineErrorMessageData));
            }

            try
            {
                this.operatorHub.Clients.All.ErrorStatusChanged(
                    machineErrorMessageData.ErrorId);
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"28:Exception {ex.Message} while sending SignalR Machine Error Message");

                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnInverterStatusWordChanged(NotificationMessage receivedMessage)
        {
            try
            {
                var msgUI = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.InverterStatusWordChanged(msgUI);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnMachineModeChanged(NotificationMessage receivedMessage)
        {
            try
            {
                if (receivedMessage.Data is DataLayer.MachineModeMessageData data)
                {
                    this.installationHub.Clients.All.MachineModeChanged(data.MachineMode);
                }
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnMoveLoadingUnit(NotificationMessage receivedMessage)
        {
            try
            {
                this.Logger.LogTrace($"13:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.MoveLoadingUnit(messageToUi);

                this.Logger.LogTrace($"14:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"15:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"16:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private async Task OnNewMissionOperationAvailable(INewMissionOperationAvailable e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            await this.operatorHub.Clients
                .All
                .NewMissionOperationAvailable(e);
        }

        private void OnPositioningChanged(NotificationMessage receivedMessage)
        {
            try
            {
                this.Logger.LogTrace($"21:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);

                this.installationHub.Clients.All.PositioningNotify(message);

                this.Logger.LogTrace($"22:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"23:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"24:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnSensorsChanged(NotificationMessage receivedMessage)
        {
            try
            {
                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.SensorsChanged(message);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void ResolutionCalibrationMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.Logger.LogTrace($"29:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ResolutionCalibrationNotify(message);

                this.Logger.LogTrace($"30:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"31:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"32:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void ShutterPositioningMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.Logger.LogTrace($"9:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ShutterPositioningNotify(message);

                this.Logger.LogTrace($"10:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"11:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"12:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void SwitchAxisMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.SwitchAxisNotify(message);
            }
            catch (ArgumentNullException exNull)
            {
                this.Logger.LogTrace($"7:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.Logger.LogTrace($"8:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        #endregion
    }
}
