using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Utilities;
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

        private readonly IHubContext<InstallationHub, IInstallationHub> hub;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private bool disposed;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public AutomationService(IEventAggregator eventAggregator, IHubContext<InstallationHub, IInstallationHub> hub, ILogger<AutomationService> logger)
        {
            logger.LogDebug("1:Method Start");
            this.eventAggregator = eventAggregator;
            this.hub = hub;

            this.logger = logger;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());

            this.InitializeMethodSubscriptions();

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

        private void CommandReceiveTaskFunction()
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
                    //case MessageType.AddMission:
                    //    this.ProcessAddMissionMessage(receivedMessage);
                    //    break;

                    //case MessageType.HorizontalHoming:
                    //    break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");
            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(commandMessage =>
                {
                    this.commandQueue.Enqueue(commandMessage);
                },
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == MessageActor.AutomationService || commandMessage.Destination == MessageActor.Any);

            this.logger.LogTrace("2:Notifications Subscription");
            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(notificationMessage =>
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

                    this.logger.LogTrace(string.Format("2:{0}:{1}:{2}",
                        receivedMessage.Type,
                        receivedMessage.Destination,
                        receivedMessage.Status));
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End - Operation Canceled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.SensorsChanged:
                        if (receivedMessage.Data is ISensorsChangedMessageData)
                        {
                            this.hub.Clients.All.OnSensorsChangedToAllConnectedClients(((ISensorsChangedMessageData)receivedMessage.Data).SensorsStates);
                        }
                        break;

                    case MessageType.Homing:
                    case MessageType.SwitchAxis:
                    case MessageType.CalibrateAxis:
                        //case MessageType.DataLayerReady:
                        //case MessageType.IOPowerUp:
                        try
                        {
                            this.logger.LogTrace($"4:Sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            var dataMessage = MessageParser.GetActionUpdateData(receivedMessage);
                            this.hub.Clients.All.OnActionUpdateToAllConnectedClients(dataMessage);
                            this.logger.LogTrace($"5:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogTrace($"6:Exception {ex.Message} while sending SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
                            throw new AutomationServiceException($"Exception: {ex.Message} while sending SignalR notification", ex);
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");

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

        #endregion

        //TEMP public async void TESTStartBoolSensorsCycle()
        //{
        //    while (true)
        //    {
        //        var message = new bool[] { (new Random().Next(10) % 2 == 0), (new Random().Next(10) % 2 == 0), (new Random().Next(10) % 2 == 0), (new Random().Next(10) % 2 == 0), };
        //        Console.WriteLine(message[0].ToString() + " " + message[1].ToString() + " " + message[2].ToString() + " " + message[3].ToString());
        //        await this.hub.Clients.All.OnSensorsChangedToAllConnectedClients(message);
        //        await Task.Delay(1000);
        //    }
        //}

        //TEMP private async void StartTestCycles()
        //{
        //    this.TESTStartBoolSensorsCycle();
        //    this.TESTStartStringMessageCycle();
        //}

        //TEMP public void SendMessageToAllConnectedClients(NotificationMessage notificationMessage)
        //{
        //    this.hub.Clients.All.OnSendMessageToAllConnectedClients(notificationMessage.Description);
        //}

        //TEMP public async void TESTStartStringMessageCycle()
        //{
        //    while (true)
        //    {
        //        var message = new[] { "pippo", "topolino", "pluto", "paperino", "minnie", "qui", "quo", "qua" };
        //        var randomInt = new Random().Next(message.Length);
        //        Console.WriteLine(message[randomInt]);
        //        await this.hub.Clients.All.OnSendMessageToAllConnectedClients(message[randomInt]);
        //        await Task.Delay(1000);
        //    }
        //}
    }
}
