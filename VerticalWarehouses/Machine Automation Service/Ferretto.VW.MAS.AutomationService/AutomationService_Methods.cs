using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService
    {
        #region Methods

        private void CalibrateAxisMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"13:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.CalibrateAxisNotify(messageToUi);

                this.logger.LogTrace($"14:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.logger.LogTrace($"15:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"16:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"5:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"5:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnBayConnected(IBayOperationalStatusChangedMessageData messageData)
        {
            if (messageData == null)
            {
                throw new ArgumentNullException(nameof(messageData));
            }

            this.operatorHub.Clients.All
                .BayStatusChanged(messageData);
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
                this.logger.LogTrace($"28:Exception {ex.Message} while sending SignalR Machine Error Message");

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
                this.logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"21:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);

                this.installationHub.Clients.All.PositioningNotify(message);

                this.logger.LogTrace($"22:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.logger.LogTrace($"23:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"24:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"3:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"4:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void OnWmsEntityChanged(object sender, EntityChangedEventArgs e)
        {
            switch (e.EntityType)
            {
                case nameof(MissionOperation):
                    {
                        if (e.Operation == WMS.Data.Hubs.Models.HubEntityOperation.Created)
                        {
                            var message = new NotificationMessage(
                                null,
                                "New mission operation from WMS",
                                MessageActor.MissionsManager,
                                MessageActor.AutomationService,
                                MessageType.NewMissionAvailable);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(message);
                        }
                        break;
                    }
            }
        }

        private void ResolutionCalibrationMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"29:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ResolutionCalibrationNotify(message);

                this.logger.LogTrace($"30:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.logger.LogTrace($"31:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"32:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void ShutterControlMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"17:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ShutterControlNotify(message);

                this.logger.LogTrace($"18:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.logger.LogTrace($"19:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"20:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private void ShutterPositioningMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"9:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ShutterPositioningNotify(message);

                this.logger.LogTrace($"10:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.logger.LogTrace($"11:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"12:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
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
                this.logger.LogTrace($"7:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"8:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        #endregion
    }
}
