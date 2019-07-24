using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService
    {
        #region Methods

        public async void TESTStartBoolSensorsCycle()
        {
            var random = new Random();
            while (true)
            {
                var sensorsState = new bool[]
                {
                    (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0)
                };

                Console.WriteLine(sensorsState[0].ToString() + " " + sensorsState[1].ToString() + " " + sensorsState[2].ToString() + " " + sensorsState[3].ToString() +
                                  sensorsState[4].ToString() + " " + sensorsState[5].ToString() + " " + sensorsState[6].ToString() + " " + sensorsState[7].ToString() +
                                  sensorsState[8].ToString() + " " + sensorsState[9].ToString() + " " + sensorsState[10].ToString() + " " + sensorsState[11].ToString() +
                                  sensorsState[12].ToString() + " " + sensorsState[13].ToString() + " " + sensorsState[14].ToString() + " " + sensorsState[15].ToString() +
                                  sensorsState[16].ToString() + " " + sensorsState[17].ToString() + " " + sensorsState[18].ToString() + " " + sensorsState[19].ToString() +
                                  sensorsState[20].ToString() + " " + sensorsState[21].ToString() + " " + sensorsState[22].ToString() + " " + sensorsState[23].ToString() +
                                  sensorsState[24].ToString() + " " + sensorsState[25].ToString() + " " + sensorsState[26].ToString() + " " + sensorsState[27].ToString() +
                                  sensorsState[28].ToString() + " " + sensorsState[29].ToString() + " " + sensorsState[30].ToString() + " " + sensorsState[31].ToString());

                var dataInterface = new SensorsChangedMessageData();
                dataInterface.SensorsStates = sensorsState;

                var notify = new NotificationMessage(dataInterface, "Sensors status", MessageActor.Any, MessageActor.AutomationService, MessageType.SensorsChanged, MessageStatus.OperationExecuting);
                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(notify);
                await this.installationHub.Clients.All.SensorsChangedNotify(messageToUI);

                await Task.Delay(1000);
            }
        }

        private void BayConnectedMethod(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is BayConnectedMessageData bayData)
            {
                var bay = this.baysManager.Bays.Where(x => x.Id == bayData.Id).First();

                var data = new BayConnectedMessageData { Id = bay.Id, BayType = (int)bay.Type, MissionQuantity = bay.Missions == null ? 0 : bay.Missions.Count };
                var message = new NotificationMessage(data, "Client Connected", MessageActor.Any, MessageActor.WebApi, MessageType.BayConnected, MessageStatus.NoStatus);
                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(message);
                this.operatorHub.Clients.Client(bay.ConnectionId).OnConnectionEstablished(messageToUI);
            }
        }

        private void CalibrateAxisMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"13:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.CalibrateAxisNotify(messageToUI);

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

        private async void DataHubClient_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            var random = new Random();
            if (!e.IsConnected)
            {
                await Task.Delay(random.Next(1, 5) * 1000);
                await this.dataHubClient.ConnectAsync();
            }
        }

        private void DataHubClient_EntityChanged(object sender, EntityChangedEventArgs e)
        {
            if (e.EntityType == "SchedulerRequest")
            {
                var message = new NotificationMessage(null, "New missions from WMS", MessageActor.MissionsManager, MessageActor.AutomationService, MessageType.MissionAdded, MessageStatus.NoStatus);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(message);
            }
        }

        private void ExceptionHandlerMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"25:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ExceptionNotify(messageToUI);

                this.logger.LogTrace($"26:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
            }
            catch (ArgumentNullException exNull)
            {
                this.logger.LogTrace($"27:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
            }
            catch (Exception ex)
            {
                this.logger.LogTrace($"28:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
            }
        }

        private async Task ExecuteMissionMethod(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is ExecuteMissionMessageData data)
            {
                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                await this.operatorHub.Clients.Client(data.BayConnectionId).ProvideMissionsToBay(messageToUI);
            }
        }

        private void HomingMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.HomingNotify(msgUI);
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

        private void PositioningMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"21:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);

                this.installationHub.Clients.All.PositioningNotify(messageToUI);

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

        private void ResolutionCalibrationMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"29:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ResolutionCalibrationNotify(messageToUI);

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

        private void SensorsChangedMethod(NotificationMessage receivedMessage)
        {
            try
            {
                var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.SensorsChangedNotify(msgUI);
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

        private void ShutterControlMethod(NotificationMessage receivedMessage)
        {
            try
            {
                this.logger.LogTrace($"17:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ShutterControlNotify(msgUI);

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

                var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.ShutterPositioningNotify(msgUI);

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
                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.installationHub.Clients.All.SwitchAxisNotify(messageToUI);
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
