using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.IODriver.IoDevices;
using Ferretto.VW.MAS.IODriver.IoDevices.Interfaces;
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
    public class HostedIoDriver : BackgroundService
    {

        #region Fields

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IConfiguration configuration;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<IoIndex, IIoDevice> ioDevices;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly IVertimagConfigurationDataLayer vertimagConfiguration;

        private bool disposed;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedIoDriver(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IVertimagConfigurationDataLayer vertimagConfiguration,
            ILogger<HostedIoDriver> logger,
            IConfiguration configuration)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.vertimagConfiguration = vertimagConfiguration;
            this.configuration = configuration;

            this.ioDevices = new Dictionary<IoIndex, IIoDevice>();

            this.commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.NotificationReceiveTaskFunction());

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Destructors

        ~HostedIoDriver()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

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

                var currentDevice = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.SwitchAxis:
                        switch (currentDevice)
                        {
                            case IoIndex.IoDevice1:
                                this.ioDevices[IoIndex.IoDevice1].ExecuteSwitchAxis(receivedMessage);
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
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void InitializeIoDevice()
        {
            var ioDevicesList = this.vertimagConfiguration.GetInstalledIoList();
            IIoDevice ioDevice = null;

            var useMockedTransport = this.configuration.GetValue<bool>("Vertimag:RemoteIODriver:UseMock");

            foreach (var ioIndex in ioDevicesList)
            {
                IIoTransport transport;
                if (!useMockedTransport)
                {
                    transport = new IoTransport();
                }
                else
                {
                    transport = new IoTransportMock();
                }

                switch (ioIndex)
                {
                    case IoIndex.IoDevice1:
                        var ipAddressDevice1 = this.dataLayerConfigurationValueManagement.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion1IPAddress, ConfigurationCategory.SetupNetwork);
                        var portDevice1 = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion1Port, ConfigurationCategory.SetupNetwork);
                        ioDevice = new IoDevice(this.eventAggregator, transport, ipAddressDevice1, portDevice1, IoIndex.IoDevice1, this.logger, this.stoppingToken);

                        break;

                    case IoIndex.IoDevice2:
                        var ipAddressDevice2 = this.dataLayerConfigurationValueManagement.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion2IPAddress, ConfigurationCategory.SetupNetwork);
                        var portDevice2 = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion2Port, ConfigurationCategory.SetupNetwork);
                        ioDevice = new IoDevice(this.eventAggregator, transport, ipAddressDevice2, portDevice2, IoIndex.IoDevice2, this.logger, this.stoppingToken);

                        break;

                    case IoIndex.IoDevice3:
                        var ipAddressDevice3 = this.dataLayerConfigurationValueManagement.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion3IPAddress, ConfigurationCategory.SetupNetwork);
                        var portDevice3 = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion3Port, ConfigurationCategory.SetupNetwork);
                        ioDevice = new IoDevice(this.eventAggregator, transport, ipAddressDevice3, portDevice3, IoIndex.IoDevice3, this.logger, this.stoppingToken);

                        break;
                }

                this.ioDevices.Add(ioIndex, ioDevice);
            }
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");

            var commandEvent = this.eventAggregator.GetEvent<FieldCommandEvent>();
            commandEvent.Subscribe(
                commandMessage => { this.commandQueue.Enqueue(commandMessage); },
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == FieldMessageActor.IoDriver || commandMessage.Destination == FieldMessageActor.Any);

            this.logger.LogTrace("1:Notifications Subscription");

            var notificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            notificationEvent.Subscribe(
                notificationMessage => { this.notificationQueue.Enqueue(notificationMessage); },
                ThreadOption.PublisherThread,
                false,
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
                this.logger.LogTrace($"Notification received: {receivedMessage.Type}, {receivedMessage.Status}, destination: {receivedMessage.Destination}");

                var currentDevice = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:
                        this.InitializeIoDevice();
                        await this.StartHardwareCommunications();

                        foreach (var ioDevice in this.ioDevices)
                        {
                            ioDevice.Value.ExecuteIoPowerUp();
                        }

                        break;

                    case FieldMessageType.IoPowerUp:
                    case FieldMessageType.SwitchAxis:
                    case FieldMessageType.PowerEnable:
                    case FieldMessageType.IoReset:
                    case FieldMessageType.ResetSecurity:
                        if (receivedMessage.Status == MessageStatus.OperationEnd &&
                            receivedMessage.ErrorLevel == ErrorLevel.NoError)
                        {
                            this.ioDevices[currentDevice].DestroyStateMachine();
                        }
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SendMessage(IFieldMessageData messageData, IoIndex deviceIndex)
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

            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
        }

        private async Task StartHardwareCommunications()
        {
            foreach (var device in this.ioDevices.Values)
            {
                await device.StartHardwareCommunications();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.Dispose();
            }

            this.disposed = true;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            this.logger.LogDebug("1:Starting Tasks");
            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while starting service threads");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0), IoIndex.None);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
