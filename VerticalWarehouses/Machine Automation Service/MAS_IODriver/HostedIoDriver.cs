using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Hosting;
using Modbus.Device;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver
{
    public class HostedIoDriver : BackgroundService
    {
        #region Fields

        private const int IoPollingInterval = 50;

        private readonly IDataLayer dataLayer;

        private readonly IEventAggregator eventAggregator;

        private readonly IModbusTransport modbusTransport;

        private readonly BlockingConcurrentQueue<IoStatus> ioCommandQueue;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly ManualResetEventSlim pollIoEvent;

        private readonly IoStatus ioStatus;

        private Task ioReceiveTask;

        private Task ioSendTask;

        private Timer pollIoTimer;

        #endregion

        #region Constructors

        public HostedIoDriver(IEventAggregator eventAggregator, IModbusTransport modbusTransport, IDataLayer dataLayer)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayer = dataLayer;
            this.modbusTransport = modbusTransport;

            this.ioStatus = new IoStatus();
            this.pollIoEvent = new ManualResetEventSlim(false);

            this.ioCommandQueue = new BlockingConcurrentQueue<IoStatus>();
            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            var messageEvent = this.eventAggregator.GetEvent<CommandEvent>();
            messageEvent.Subscribe(message => { this.messageQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.IODriver || message.Destination == MessageActor.Any);
        }

        #endregion

        #region Methods

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            var returnValue = base.StopAsync(stoppingToken);

            return returnValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => this.HostedIoDriverTaskFunction(stoppingToken), stoppingToken);
        }

        private Task HostedIoDriverTaskFunction(CancellationToken stoppingToken)
        {
            var ioAddress = this.dataLayer.GetIPAddressConfigurationValue(ConfigurationValueEnum.IoAddress);
            var ioPort = this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.IoPort);

            modbusTransport.Configure(ioAddress, ioPort);

            bool connectionResult;
            try
            {
                connectionResult = this.modbusTransport.Connect();
            }
            catch (Exception ex)
            {
                throw new IoDriverException($"Exception: {ex.Message} while connecting to Modbus I/O master", IoDriverExceptionCode.CreationFailure, ex);
            }

            if (!connectionResult)
            {
                throw new IoDriverException("Failed to connect to Modbus I/O master");
            }

            this.pollIoTimer?.Dispose();
            this.pollIoTimer = new Timer(ReadIoData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IoPollingInterval));

            this.ioSendTask?.Dispose();
            this.ioSendTask = Task.Run(() => this.SendIoCommand(stoppingToken), stoppingToken);

            this.ioReceiveTask?.Dispose();
            this.ioReceiveTask = Task.Run(() => this.ReceiveIoData(stoppingToken), stoppingToken);

            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.messageQueue.TryDequeue(Timeout.Infinite, stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return Task.CompletedTask;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.Calibrate:

                        break;
                }
            } while (stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ReadIoData(object state)
        {
            this.pollIoEvent.Set();
        }

        private async Task ReceiveIoData(CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    pollIoEvent.Wait(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var inputData = await this.modbusTransport.ReadAsync();

                if (this.ioStatus.UpdateInputStates(inputData))
                {
                    this.eventAggregator.GetEvent<NotificationEvent>();
                }
            } while (stoppingToken.IsCancellationRequested);
        }

        private async Task SendIoCommand(CancellationToken stoppingToken)
        {
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.messageQueue.TryDequeue(Timeout.Infinite, stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.Calibrate:
                        break;
                }
            } while (!stoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
