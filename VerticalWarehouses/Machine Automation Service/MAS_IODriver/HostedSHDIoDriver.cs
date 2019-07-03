using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_IODriver
{
    public class HostedSHDIoDriver : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<IoIndex, IIoDevice> ioDevices;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly IVertimagConfiguration vertimagConfiguration;

        private bool disposed;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedSHDIoDriver(
            IEventAggregator eventAggregator,
            IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement,
            IVertimagConfiguration vertimagConfiguration,
            ILogger<HostedSHDIoDriver> logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.vertimagConfiguration = vertimagConfiguration;

            this.ioDevices = new Dictionary<IoIndex, IIoDevice>();

            this.commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.NotificationReceiveTaskFunction());

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Destructors

        ~HostedSHDIoDriver()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                base.Dispose();
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

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0));
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

                IoIndex currentDevice;
                switch (receivedMessage.Type)
                {
                    case FieldMessageType.SwitchAxis:
                        switch (receivedMessage.DeviceIndex)
                        {
                            case (byte)IoIndex.IoDevice1:
                                this.ioDevices[IoIndex.IoDevice1].ExecuteSwitchAxis(receivedMessage);
                                break;

                            default:
                                break;
                        }
                        break;

                    case FieldMessageType.IoReset:
                        currentDevice = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());
                        this.ioDevices[currentDevice].ExecuteIoReset();
                        break;

                    case FieldMessageType.SensorsChanged:
                        currentDevice = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());
                        this.ioDevices[currentDevice].ExecuteSensorsStateUpdate(receivedMessage);
                        break;

                    default:
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task InitializeIoDevice()
        {
            var ioDevicesList = await this.vertimagConfiguration.GetInstalledIoListAsync();
            IIoDevice ioDevice = null;

            foreach (var ioIndex in ioDevicesList)
            {
                switch (ioIndex)
                {
                    case IoIndex.IoDevice1:
                        var ipAddressDevice1 = await this.dataLayerConfigurationValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);
                        var portDevice1 = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);
                        ioDevice = new IoDevice(this.eventAggregator, ipAddressDevice1, portDevice1, IoIndex.IoDevice1, this.logger);
                        break;

                    case IoIndex.IoDevice2:
                        var ipAddressDevice2 = await this.dataLayerConfigurationValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion2, (long)ConfigurationCategory.SetupNetwork);
                        var portDevice2 = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion2Port, (long)ConfigurationCategory.SetupNetwork);
                        ioDevice = new IoDevice(this.eventAggregator, ipAddressDevice2, portDevice2, IoIndex.IoDevice2, this.logger);
                        break;

                    case IoIndex.IoDevice3:
                        var ipAddressDevice3 = await this.dataLayerConfigurationValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion3, (long)ConfigurationCategory.SetupNetwork);
                        var portDevice3 = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion3Port, (long)ConfigurationCategory.SetupNetwork);
                        ioDevice = new IoDevice(this.eventAggregator, ipAddressDevice3, portDevice3, IoIndex.IoDevice3, this.logger);
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
                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:
                        await this.InitializeIoDevice();
                        await this.StartHardwareCommunications();

                        foreach (var ioDevice in this.ioDevices)
                        {
                            ioDevice.Value.ExecuteIoPowerUp();
                        }

                        break;

                    case FieldMessageType.IoPowerUp:
                    case FieldMessageType.SwitchAxis:
                        if (receivedMessage.Status == MessageStatus.OperationEnd &&
                            receivedMessage.ErrorLevel == ErrorLevel.NoError)
                        {
                            var index = (IoIndex)receivedMessage.DeviceIndex;
                            this.ioDevices[index].DestroyStateMachine();
                        }
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SendMessage(IFieldMessageData messageData)
        {
            var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
            messageData,
            "Io Driver Error",
            FieldMessageActor.Any,
            FieldMessageActor.IoDriver,
            FieldMessageType.IoDriverException,
            MessageStatus.OperationError,
            ErrorLevel.Critical);

            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
        }

        private async Task StartHardwareCommunications()
        {
            foreach (var device in this.ioDevices.Values)
            {
                device.StartHardwareCommunications();
            }
        }

        #endregion
    }
}
