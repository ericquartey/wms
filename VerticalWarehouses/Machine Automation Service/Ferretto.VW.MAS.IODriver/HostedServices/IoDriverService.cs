using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Microsoft.Extensions.Hosting;
using Ferretto.VW.MAS.Utils.Utilities;
using System.Threading;

namespace Ferretto.VW.MAS.IODriver
{
    internal sealed class IoDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();

        private readonly IConfiguration configuration;

        private readonly IHostEnvironment env;

        private readonly Dictionary<DataModels.IoIndex, IIoDevice> ioDevices = new Dictionary<DataModels.IoIndex, IIoDevice>();

        private readonly IIoDevicesProvider ioDeviceService;

        #endregion

        #region Constructors

        public IoDriverService(
            IEventAggregator eventAggregator,
            IIoDevicesProvider iIoDeviceService,
            ILogger<IoDriverService> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHostEnvironment env)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.ioDeviceService = iIoDeviceService ?? throw new ArgumentNullException(nameof(iIoDeviceService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.env = env ?? throw new ArgumentNullException(nameof(env));
        }

        #endregion

        #region Methods

        protected override bool FilterCommand(FieldCommandMessage command)
        {
            return
                command.Destination is FieldMessageActor.IoDriver
                ||
                command.Destination is FieldMessageActor.Any;
        }

        protected override bool FilterNotification(FieldNotificationMessage notification)
        {
            return
                notification.Destination is FieldMessageActor.IoDriver
                ||
                notification.Destination is FieldMessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(FieldCommandMessage command, IServiceProvider serviceProvider)
        {
            var currentDevice = Enum.Parse<DataModels.IoIndex>(command.DeviceIndex.ToString());
            var ioDevice = this.ioDevices[currentDevice];

            if (ioDevice.IsCommandExecuting)
            {
                // Enqueue the incoming filed command message
                this.commandQueue.Enqueue(command);
                return Task.CompletedTask;
            }

            switch (command.Type)
            {
                case FieldMessageType.SwitchAxis:
                    if (currentDevice is DataModels.IoIndex.IoDevice1)
                    {
                        ioDevice.ExecuteSwitchAxis(command);
                    }

                    break;

                case FieldMessageType.IoReset:
                    ioDevice.ExecuteIoReset();
                    break;

                case FieldMessageType.SensorsChanged:
                    ioDevice.ExecuteSensorsStateUpdate(command);
                    break;

                case FieldMessageType.ResetSecurity:
                    ioDevice.ExecuteResetSecurity();
                    break;

                case FieldMessageType.PowerEnable:
                    ioDevice.ExecutePowerEnable(command);
                    break;

                case FieldMessageType.MeasureProfile:
                    ioDevice.ExecuteMeasureProfile(command);
                    break;

                case FieldMessageType.BayLight:
                    ioDevice.ExecuteBayLight(command);
                    break;
            }

            return Task.CompletedTask;
        }

        protected override async Task OnNotificationReceivedAsync(FieldNotificationMessage message, IServiceProvider serviceProvider)
        {
            var currentDevice = Enum.Parse<DataModels.IoIndex>(message.DeviceIndex.ToString());

            if (message.Type is FieldMessageType.DataLayerReady)
            {
                this.InitializeIoDevice();
                await this.StartHardwareCommunicationsAsync();

                foreach (var ioDevice in this.ioDevices.Values)
                {
                    ioDevice.ExecuteIoPowerUp();
                }
            }

            if (message.Source is FieldMessageActor.IoDriver
                &&
                message.Destination is FieldMessageActor.IoDriver
                &&
                (message.Status is MessageStatus.OperationEnd
                ||
                message.Status is MessageStatus.OperationError
                ||
                message.Status is MessageStatus.OperationStop))
            {
                this.ioDevices[currentDevice].DestroyStateMachine();

                if (message.Status == MessageStatus.OperationError && message.ErrorLevel == ErrorLevel.Error && message.Type == FieldMessageType.ResetSecurity)
                {
                    this.ioDevices[currentDevice].Disconnect();
                }

                // forward the message to upper level
                message.Destination = FieldMessageActor.DeviceManager;

                this.EventAggregator
                    .GetEvent<FieldNotificationEvent>()
                    .Publish(message);

                // Check if other command is enqueued and publish it into background service
                if (this.commandQueue.Count > 0)
                {
                    if (this.commandQueue.TryPeek(Timeout.Infinite, this.CancellationToken, out var fieldCommandMessage) &&
                        fieldCommandMessage != null)
                    {
                        this.commandQueue.Dequeue(out _);
                        this.Logger.LogDebug($"Publish the {fieldCommandMessage.Type} command message from {fieldCommandMessage.Source} to {fieldCommandMessage.Destination}");
                        this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(fieldCommandMessage);
                    }
                }
            }
        }

        private void InitializeIoDevice()
        {
            var useMockedTransport = this.configuration.GetValue<bool>("Vertimag:Drivers:RemoteIO:UseMock");
            var readTimeoutMilliseconds = this.configuration.GetValue("Vertimag:Drivers:RemoteIO:ReadTimeoutMilliseconds", -1);

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var bayDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                var digitalDevicesDataProvider = scope.ServiceProvider.GetRequiredService<IDigitalDevicesDataProvider>();
                var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                var ioDevices = digitalDevicesDataProvider.GetAllIoDevices();
                var mainIoDevice = ioDevices.SingleOrDefault(d => d.Index == DataModels.IoIndex.IoDevice1);

                foreach (var ioDevice in ioDevices)
                {
                    var transport = useMockedTransport
                        ? (IIoTransport)new IoTransportMock()
                        : new IoTransport(readTimeoutMilliseconds);

                    this.ioDevices.Add(
                        ioDevice.Index,
                        new IoDevice(
                            this.EventAggregator,
                            this.ioDeviceService,
                            this.ServiceScopeFactory,
                            transport,
                            ioDevice.IpAddress,
                            ioDevice.TcpPort,
                            ioDevice.Index,
                            bayDataProvider.GetByIoIndex(ioDevice.Index),
                            this.Logger,
                            this.CancellationToken,
                            this.env));
                }
            }
        }

        private async Task StartHardwareCommunicationsAsync()
        {
            foreach (var device in this.ioDevices.Values)
            {
                await device.StartHardwareCommunicationsAsync();
            }
        }

        #endregion
    }
}
