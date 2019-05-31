using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private IDataHubClient dataHubClient;

        private bool disposed;

        private IMachinesDataService machinesDataService;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public AutomationService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<AutomationService> logger,
            IDataHubClient dataHubClient,
            IMachinesDataService machinesDataService
            )
        {
            logger.LogDebug("1:Method Start");
            this.eventAggregator = eventAggregator;
            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.machinesDataService = machinesDataService;

            this.logger = logger;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());

            this.InitializeMethodSubscriptions();
            //this.dataHubClient.ConnectAsync();

            this.dataHubClient.ConnectionStatusChanged += this.DataHubClient_ConnectionStatusChanged;
            this.dataHubClient.EntityChanged += this.DataHubClient_EntityChanged;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~AutomationService()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
        }

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new AutomationServiceException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private async void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                    this.logger.LogTrace($"2:Dequeued Message:{receivedMessage.Type}:Destination{receivedMessage.Source}");
                }
                catch (OperationCanceledException ex)
                {
                    this.logger.LogDebug("3:Method End - Operation Canceled");
                    return;
                }
                switch (receivedMessage.Type)
                {
                    case MessageType.MissionManagerInitialized:
                        var missions = await this.machinesDataService.GetMissionsByIdAsync(1);
                        var messageData = new MissionMessageData(missions);
                        var message = new CommandMessage(messageData, "New missions from WMS", MessageActor.MissionsManager, MessageActor.AutomationService, MessageType.MissionAdded);
                        this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");
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
                var missions = await this.machinesDataService.GetMissionsByIdAsync(1);
                var messageData = new MissionMessageData(missions);
                var message = new CommandMessage(messageData, "New missions from WMS", MessageActor.MissionsManager, MessageActor.AutomationService, MessageType.MissionAdded);
                this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
            }
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");
            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(
                commandMessage =>
                {
                    this.commandQueue.Enqueue(commandMessage);
                },
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == MessageActor.AutomationService || commandMessage.Destination == MessageActor.Any);

            this.logger.LogTrace("2:Notifications Subscription");
            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                notificationMessage =>
                {
                    this.notificationQueue.Enqueue(notificationMessage);
                },
                ThreadOption.PublisherThread,
                false,
                notificationMessage => notificationMessage.Destination == MessageActor.AutomationService || notificationMessage.Destination == MessageActor.Any);
        }

        private void NotificationReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End - Operation Canceled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.SensorsChanged:
                        try
                        {
                            var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.SensorsChangedNotify(msgUI);
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"9:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.Homing:
                        try
                        {
                            var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.HomingNotify(msgUI);
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"10:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.SwitchAxis:
                        try
                        {
                            var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.SwitchAxisNotify(messageToUI);
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"11:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.ShutterPositioning:
                        try
                        {
                            this.logger.LogTrace($"4:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                            var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.ShutterPositioningNotify(msgUI);

                            this.logger.LogTrace($"5:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"12:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.CalibrateAxis:
                        //case MessageType.DataLayerReady:
                        //case MessageType.IOPowerUp:
                        try
                        {
                            this.logger.LogTrace($"4:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                            var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.CalibrateAxisNotify(messageToUI);

                            this.logger.LogTrace($"5:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"12:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.ShutterControl:
                        try
                        {
                            this.logger.LogTrace($"4:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                            var msgUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.ShutterControlNotify(msgUI);

                            this.logger.LogTrace($"5:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"12:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.Positioning:
                        try
                        {
                            this.logger.LogTrace($"14:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                            var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);

                            this.installationHub.Clients.All.VerticalPositioningNotify(messageToUI);

                            this.logger.LogTrace($"15:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"12:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.FSMException:
                    case MessageType.InverterException:
                    case MessageType.IoDriverException:
                    case MessageType.DLException:
                        try
                        {
                            this.logger.LogTrace($"14:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                            var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.ExceptionNotify(messageToUI);

                            this.logger.LogTrace($"15:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"12:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    case MessageType.ResolutionCalibration:
                        try
                        {
                            this.logger.LogTrace($"14:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");

                            var messageToUI = NotificationMessageUIFactory.FromNotificationMessage(receivedMessage);
                            this.installationHub.Clients.All.ResolutionCalibrationNotify(messageToUI);

                            this.logger.LogTrace($"15:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (ArgumentNullException exNull)
                        {
                            this.logger.LogTrace($"12:Exception {exNull.Message} while create SignalR Message:{receivedMessage.Type}");
                            throw new AutomationServiceException($"Exception: {exNull.Message} while sending SignalR notification", exNull);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;

                    // Adds other Notification Message and send it via SignalR controller
                    default:
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("9:Method End");

            return;
        }

        private void ProcessAddMissionMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            message.Source = MessageActor.AutomationService;
            message.Destination = MessageActor.MissionsManager;
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);

            this.logger.LogDebug("2:Method End");
        }

        /// <summary>
        /// Test for sensor status update.
        /// </summary>
        private void StartTestCycles()
        {
            this.TESTStartBoolSensorsCycle();
        }

        #endregion
    }
}
