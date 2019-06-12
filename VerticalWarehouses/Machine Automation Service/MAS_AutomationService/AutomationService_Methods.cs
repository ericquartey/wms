using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_AutomationService
{
    public partial class AutomationService
    {
        #region Methods

        public async void TESTStartBoolSensorsCycle()
        {
            var random = new Random();
            while (true)
            {
                var SensorsState = new bool[] { (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0),
                                                 (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0), (random.Next(10) % 2 == 0)};

                Console.WriteLine(SensorsState[0].ToString() + " " + SensorsState[1].ToString() + " " + SensorsState[2].ToString() + " " + SensorsState[3].ToString() +
                                  SensorsState[4].ToString() + " " + SensorsState[5].ToString() + " " + SensorsState[6].ToString() + " " + SensorsState[7].ToString() +
                                  SensorsState[8].ToString() + " " + SensorsState[9].ToString() + " " + SensorsState[10].ToString() + " " + SensorsState[11].ToString() +
                                  SensorsState[12].ToString() + " " + SensorsState[13].ToString() + " " + SensorsState[14].ToString() + " " + SensorsState[15].ToString() +
                                  SensorsState[16].ToString() + " " + SensorsState[17].ToString() + " " + SensorsState[18].ToString() + " " + SensorsState[19].ToString() +
                                  SensorsState[20].ToString() + " " + SensorsState[21].ToString() + " " + SensorsState[22].ToString() + " " + SensorsState[23].ToString() +
                                  SensorsState[24].ToString() + " " + SensorsState[25].ToString() + " " + SensorsState[26].ToString() + " " + SensorsState[27].ToString() +
                                  SensorsState[28].ToString() + " " + SensorsState[29].ToString() + " " + SensorsState[30].ToString() + " " + SensorsState[31].ToString());

                var dataInterface = new SensorsChangedMessageData();
                dataInterface.SensorsStates = SensorsState;

                var notify = new NotificationMessage(dataInterface, "Sensors status", MessageActor.Any, MessageActor.AutomationService, MessageType.SensorsChanged, MessageStatus.OperationExecuting);
                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(notify);
                await this.installationHub.Clients.All.SensorsChangedNotify(messageToUI);

                await Task.Delay(1000);
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

        private async void DataHubClient_EntityChanged(object sender, EntityChangedEventArgs e)
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

        private void ExecuteMissionMethod(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is ExecuteMissionMessageData data)
            {
                var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                this.operatorHub.Clients.Client(data.BayConnectionId).ProvideMissionsToBay(messageToUI);
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

                this.installationHub.Clients.All.VerticalPositioningNotify(messageToUI);

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
