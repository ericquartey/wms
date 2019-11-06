using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver
{
    internal sealed class IoDriverService : BackgroundService
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();

        private readonly Task commandReceiveTask;

        private readonly IConfiguration configuration;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly IIoDevicesProvider iIoDeviceService;

        private readonly Dictionary<DataModels.IoIndex, IIoDevice> ioDevices = new Dictionary<DataModels.IoIndex, IIoDevice>();

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

        private readonly Task notificationReceiveTask;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public IoDriverService(
            IEventAggregator eventAggregator,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IBaysProvider baysProvider,
            IIoDevicesProvider iIoDeviceService,
            ILogger<IoDriverService> logger,
            IConfiguration configuration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.iIoDeviceService = iIoDeviceService ?? throw new ArgumentNullException(nameof(iIoDeviceService));
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.NotificationReceiveTaskFunction());

            logger.LogTrace("1:Method Start");

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Methods

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            this.logger.LogTrace("1:Starting Tasks");
            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while starting service threads");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0), DataModels.IoIndex.None);
            }

            return Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            do
            {
                FieldCommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Type={receivedMessage.Type}:Destination={receivedMessage.Destination}:receivedMessage={receivedMessage}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End - Operation Canceled");

                    return;
                }

                this.logger.LogTrace($"3:Filed Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}");

                var currentDevice = Enum.Parse<DataModels.IoIndex>(receivedMessage.DeviceIndex.ToString());

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.SwitchAxis:
                        switch (currentDevice)
                        {
                            case DataModels.IoIndex.IoDevice1:
                                this.ioDevices[DataModels.IoIndex.IoDevice1].ExecuteSwitchAxis(receivedMessage);
                                break;
                        }

                        break;

                    case FieldMessageType.IoReset:
                        this.ioDevices[currentDevice].ExecuteIoReset();
                        break;

                    case FieldMessageType.SensorsChanged:
                        this.ioDevices[currentDevice].ExecuteSensorsStateUpdate(receivedMessage);
                        break;

                    case FieldMessageType.ResetSecurity:
                        this.ioDevices[currentDevice].ExecuteResetSecurity();
                        break;

                    case FieldMessageType.PowerEnable:
                        this.ioDevices[currentDevice].ExecutePowerEnable(receivedMessage);
                        break;

                    case FieldMessageType.MeasureProfile:
                        this.ioDevices[currentDevice].ExecuteMeasureProfile(receivedMessage);
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void InitializeIoDevice()
        {
            var ioDevices = this.digitalDevicesDataProvider.GetAllIoDevices();

            var useMockedTransport = this.configuration.GetValue<bool>("Vertimag:RemoteIODriver:UseMock");
            var readTimeoutMilliseconds = this.configuration.GetValue("Vertimag:RemoteIODriver:ReadTimeoutMilliseconds", -1);

            var mainIoDevice = ioDevices.SingleOrDefault(d => d.Index == DataModels.IoIndex.IoDevice1);

            foreach (var ioDevice in ioDevices)
            {
                var transport = useMockedTransport ? (IIoTransport)new IoTransportMock() : new IoTransport(readTimeoutMilliseconds);
                var isCarousel = this.baysProvider.GetByIoIndex(ioDevice.Index).Carousel != null;

                this.ioDevices.Add(
                    ioDevice.Index,
                    new IoDevice(
                        this.eventAggregator,
                        this.iIoDeviceService,
                        transport,
                        ioDevice.IpAddress,
                        ioDevice.TcpPort,
                        ioDevice.Index,
                        isCarousel,
                        this.logger,
                        this.stoppingToken));
            }
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");

            var commandEvent = this.eventAggregator.GetEvent<FieldCommandEvent>();
            commandEvent.Subscribe(
                commandMessage => { this.commandQueue.Enqueue(commandMessage); },
                ThreadOption.PublisherThread,
                true,
                commandMessage => commandMessage.Destination == FieldMessageActor.IoDriver || commandMessage.Destination == FieldMessageActor.Any);

            this.logger.LogTrace("1:Notifications Subscription");

            var notificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            notificationEvent.Subscribe(
                notificationMessage => { this.notificationQueue.Enqueue(notificationMessage); },
                ThreadOption.PublisherThread,
                true,
                notificationMessage => notificationMessage.Destination == FieldMessageActor.IoDriver || notificationMessage.Destination == FieldMessageActor.Any);
        }

        private async Task NotificationReceiveTaskFunction()
        {
            do
            {
                FieldNotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                    this.logger.LogTrace($"1:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }

                await this.OnFieldNotificationReceived(receivedMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task OnFieldNotificationReceived(FieldNotificationMessage receivedMessage)
        {
            this.logger.LogTrace($"Notification received: {receivedMessage.Type}, {receivedMessage.Status}, destination: {receivedMessage.Destination}");

            var currentDevice = Enum.Parse<DataModels.IoIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Type == FieldMessageType.DataLayerReady)
            {
                this.InitializeIoDevice();
                await this.StartHardwareCommunications();

                foreach (var ioDevice in this.ioDevices.Values)
                {
                    ioDevice.ExecuteIoPowerUp();
                }
            }

            if (receivedMessage.Source == FieldMessageActor.IoDriver
                &&
                receivedMessage.Destination == FieldMessageActor.IoDriver
                &&
                (receivedMessage.Status == MessageStatus.OperationEnd
                ||
                receivedMessage.Status == MessageStatus.OperationError
                ||
                receivedMessage.Status == MessageStatus.OperationStop))
            {
                this.ioDevices[currentDevice].DestroyStateMachine();

                // forward the message to upper level
                receivedMessage.Destination = FieldMessageActor.DeviceManager;

                this.eventAggregator
                    .GetEvent<FieldNotificationEvent>()
                    .Publish(receivedMessage);
            }
        }

        private void SendMessage(IFieldMessageData messageData, DataModels.IoIndex deviceIndex)
        {
            var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
            messageData,
            "Io Driver Error",
            FieldMessageActor.Any,
            FieldMessageActor.IoDriver,
            FieldMessageType.IoDriverException,
            MessageStatus.OperationError,
            (byte)deviceIndex,
            ErrorLevel.Critical);

            this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
        }

        private async Task StartHardwareCommunications()
        {
            foreach (var device in this.ioDevices.Values)
            {
                await device.StartHardwareCommunications();
            }
        }

        #endregion
    }
}
