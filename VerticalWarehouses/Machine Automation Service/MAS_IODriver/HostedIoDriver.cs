using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_IODriver.StateMachines;
using Ferretto.VW.MAS_IODriver.StateMachines.PowerUp;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver
{
    public class HostedIoDriver : BackgroundService
    {
        #region Fields

        private const int IoPollingInterval = 50;

        private readonly Task commadReceiveTask;

        private readonly IDataLayer dataLayer;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<IoMessage> ioCommandQueue;

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoStatus ioStatus;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly IModbusTransport modbusTransport;

        private readonly ManualResetEventSlim pollIoEvent;

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        private Timer pollIoTimer;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedIoDriver(IEventAggregator eventAggregator, IModbusTransport modbusTransport, IDataLayer dataLayer)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayer = dataLayer;
            this.modbusTransport = modbusTransport;

            this.ioStatus = new IoStatus();
            this.pollIoEvent = new ManualResetEventSlim(false);

            this.ioCommandQueue = new BlockingConcurrentQueue<IoMessage>();

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.commadReceiveTask = new Task(() => CommandReceiveTaskFunction());
            this.ioReceiveTask = new Task(async () => await this.ReceiveIoData());
            this.ioSendTask = new Task(async () => await this.SendIoCommand());

            var messageEvent = this.eventAggregator.GetEvent<CommandEvent>();
            messageEvent.Subscribe(message => { this.messageQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.IODriver || message.Destination == MessageActor.Any);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            base.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.pollIoTimer?.Dispose();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

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

            try
            {
                this.commadReceiveTask.Start();
                this.ioReceiveTask.Start();
                this.ioSendTask.Start();
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private Task CommandReceiveTaskFunction()
        {
            this.pollIoTimer?.Dispose();
            this.pollIoTimer = new Timer(ReadIoData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IoPollingInterval));

            this.currentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.eventAggregator);
            this.currentStateMachine.Start();

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
                    case MessageType.SwitchAxis:

                        break;
                }
            } while (stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ReadIoData(object state)
        {
            this.pollIoEvent.Set();
        }

        private async Task ReceiveIoData()
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
                    this.currentStateMachine.ProcessMessage(new IoMessage(inputData, true));
                }
            } while (stoppingToken.IsCancellationRequested);
        }

        private async Task SendIoCommand()
        {
            do
            {
                IoMessage message;
                try
                {
                    this.ioCommandQueue.TryDequeue(Timeout.Infinite, stoppingToken, out message);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (message.ValidOutputs)
                {
                    if (this.ioStatus.UpdateOutputStates(message.Outputs) || message.Force)
                    {
                        await this.modbusTransport.WriteAsync(message.Outputs);
                        this.currentStateMachine.ProcessMessage(message);
                    }
                }
            } while (!stoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
