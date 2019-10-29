using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable;
using Ferretto.VW.MAS.DeviceManager.PowerEnable;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ResetFault;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.DeviceManager
{
    internal partial class DeviceManager : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue = new BlockingConcurrentQueue<CommandMessage>();

        private readonly Task commandReceiveTask;

        private readonly Dictionary<BayNumber, IStateMachine> currentStateMachines = new Dictionary<BayNumber, IStateMachine>();

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

        private readonly Task fieldNotificationReceiveTask;

        private readonly ILogger<DeviceManager> logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

        private readonly Task notificationReceiveTask;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool forceInverterIoStatusPublish;

        private bool[] forceRemoteIoStatusPublish;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public DeviceManager(
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.commandReceiveTask = new Task(this.CommandReceiveTaskFunction);
            this.notificationReceiveTask = new Task(this.NotificationReceiveTaskFunction);
            this.fieldNotificationReceiveTask = new Task(this.FieldNotificationReceiveTaskFunction);

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogTrace("1:Method Start");

            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
                this.fieldNotificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while starting service threads");

                this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
            }

            await Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                do
                {
                    try
                    {
                        this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage);

                        this.logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, TargetBay:{receivedMessage.TargetBay}");

                        this.OnCommandReceived(receivedMessage, scope.ServiceProvider);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"2:Exception: {ex.Message}");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                        return;
                    }
                }
                while (!this.stoppingToken.IsCancellationRequested);
            }
        }

        private void FieldNotificationReceiveTaskFunction()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                do
                {
                    try
                    {
                        this.fieldNotificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage);

                        this.logger.LogTrace($"1:Queue Length({this.fieldNotificationQueue.Count}), Field Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");

                        this.OnFieldNotificationReceived(receivedMessage, scope.ServiceProvider);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"2:Exception: {ex.Message}");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                        return;
                    }
                }
                while (!this.stoppingToken.IsCancellationRequested);
            }
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Subscription Command");

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(
                message =>
                {
                    this.logger.LogTrace($"Enqueue Command message: {message.Type}, Source: {message.Source}, Destination {message.Destination}");
                    this.commandQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.DeviceManager || message.Destination == MessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                message =>
                {
                    this.logger.LogTrace($"Enqueue Notification message: {message.Type}, Source: {message.Source}, Destination {message.Destination}, Status: {message.Status}");
                    this.notificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.DeviceManager || message.Destination == MessageActor.Any);

            var fieldNotificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            fieldNotificationEvent.Subscribe(
                message =>
                {
                    this.logger.LogTrace($"Enqueue Field Notification message: {message.Type}, Source: {message.Source}, Destination {message.Destination}, Status: {message.Status}, Count: {this.fieldNotificationQueue.Count}");
                    this.fieldNotificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.DeviceManager || message.Destination == FieldMessageActor.Any);
        }

        private void MachineSensorsStatusOnFaultStateChanged(object sender, StatusUpdateEventArgs e)
        {
            this.logger.LogError($"Inverter Fault signal detected! Begin Stop machine procedure.");
            var messageData = new StateChangedMessageData(e.NewState);
            var msg = new NotificationMessage(
                messageData,
                "FSM Error",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.FaultStateChanged,
                BayNumber.None);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var inverterProvider = scope.ServiceProvider.GetRequiredService<IInvertersProvider>();
                foreach (var inverter in inverterProvider.GetAll())
                {
                    var fieldMessageData = new InverterCurrentErrorFieldMessageData();
                    var commandMessage = new FieldCommandMessage(
                        fieldMessageData,
                        $"Request Inverter Error Code",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.InverterCurrentError,
                        (byte)inverter.SystemIndex);

                    this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(commandMessage);
                }
            }
        }

        private void MachineSensorsStatusOnRunningStateChanged(object sender, StatusUpdateEventArgs e)
        {
            if (!e.NewState)
            {
                this.logger.LogError($"RunningState signal fall detected! Begin Stop machine procedure.");
            }
            var messageData = new StateChangedMessageData(e.NewState);
            var msg = new NotificationMessage(
                messageData,
                "FSM Error",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.RunningStateChanged,
                BayNumber.None);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        private void NotificationReceiveTaskFunction()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                do
                {
                    try
                    {
                        this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage);

                        this.logger.LogTrace($"1:Queue Length ({this.notificationQueue.Count}), Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status} TargetBay:{receivedMessage.TargetBay}");

                        this.OnNotificationReceived(receivedMessage, scope.ServiceProvider);
                    }
                    catch (OperationCanceledException)
                    {
                        this.logger.LogDebug("2:Method End operation cancelled");

                        return;
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"3:Exception: {ex.Message}");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                        return;
                    }
                }
                while (!this.stoppingToken.IsCancellationRequested);
            }
        }

        private void OnCommandReceived(CommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            if (!this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var messageCurrentStateMachine))
            {
                messageCurrentStateMachine = null;
            }

            if (messageCurrentStateMachine != null
                && receivedMessage.Type != MessageType.Stop
                && receivedMessage.Type != MessageType.SensorsChanged
                && receivedMessage.Type != MessageType.PowerEnable
                && receivedMessage.Type != MessageType.ContinueMovement)
            {
                var errorNotification = new NotificationMessage(
                    receivedMessage.Data,
                    $"Bay {receivedMessage.RequestingBay} is already executing the machine {messageCurrentStateMachine.GetType().Name}",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    receivedMessage.Type,
                    receivedMessage.RequestingBay,
                    receivedMessage.RequestingBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.logger.LogWarning($"Bay {receivedMessage.RequestingBay} is already executing the machine {messageCurrentStateMachine.GetType().Name}");
                this.logger.LogError($"Message [{receivedMessage.Type}] will be discarded!");

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(errorNotification);
                return;
            }

            this.logger.LogInformation($"Processing command [{receivedMessage.Type}] by {receivedMessage.RequestingBay} for {receivedMessage.TargetBay}");
            switch (receivedMessage.Type)
            {
                case MessageType.ContinueMovement:
                    this.ProcessContinueMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.Homing:
                    this.ProcessHomingMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.Stop:
                    this.ProcessStopMessage(receivedMessage);
                    break;

                case MessageType.ShutterPositioning:
                    this.ProcessShutterPositioningMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.Positioning:
                    this.ProcessPositioningMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.SensorsChanged:
                    this.ProcessSensorsChangedMessage(serviceProvider);
                    break;

                case MessageType.CheckCondition:
                    this.ProcessCheckConditionMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.PowerEnable:
                    this.ProcessPowerEnableMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.InverterStop:
                    this.ProcessInverterStopMessage();
                    break;

                case MessageType.InverterFaultReset:
                    this.ProcessInverterFaultResetMessage(receivedMessage, serviceProvider);
                    break;

                case MessageType.ResetSecurity:
                    this.ProcessResetSecurityMessage(receivedMessage);
                    break;

                case MessageType.InverterPowerEnable:
                    this.ProcessInverterPowerEnable(receivedMessage, serviceProvider);
                    break;
            }

            var notificationMessageData = new MachineStatusActiveMessageData(
                MessageActor.DeviceManager,
                receivedMessage.Type.ToString(),
                MessageVerbosity.Info);

            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"FSM current machine status {receivedMessage.Type}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.MachineStatusActive,
                receivedMessage.RequestingBay,
                receivedMessage.RequestingBay,
                MessageStatus.OperationStart);

            messageCurrentStateMachine?.PublishNotificationMessage(notificationMessage);
        }

        private void OnFieldNotificationReceived(FieldNotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            if (receivedMessage is null)
            {
                return;
            }

            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var baysProvider = serviceProvider.GetRequiredService<IBaysProvider>();

            var messageBayBayIndex = BayNumber.None;

            if (receivedMessage.Source is FieldMessageActor.IoDriver)
            {
                var messageIoIndex = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());
                messageBayBayIndex = baysProvider.GetByIoIndex(messageIoIndex, receivedMessage.Type);
            }
            else if (receivedMessage.Source is FieldMessageActor.InverterDriver)
            {
                var messageInverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
                messageBayBayIndex = baysProvider.GetByInverterIndex(messageInverterIndex);
            }

            var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();
            switch (receivedMessage.Type)
            {
                case FieldMessageType.CalibrateAxis:
                case FieldMessageType.InverterPowerOff:
                    break;

                case FieldMessageType.SensorsChanged:

                    this.logger.LogTrace($"3:IOSensorsChanged received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}, data {receivedMessage.Data}");
                    if (receivedMessage.Data is ISensorsChangedFieldMessageData dataIOs)
                    {
                        var ioIndex = receivedMessage.DeviceIndex;
                        if (machineResourcesProvider.UpdateInputs(ioIndex, dataIOs.SensorsStates, receivedMessage.Source) || this.forceRemoteIoStatusPublish[ioIndex])
                        {
                            var msgData = new SensorsChangedMessageData
                            {
                                SensorsStates = machineResourcesProvider.DisplayedInputs
                            };

                            this.logger.LogTrace($"FSM: IoIndex {ioIndex}, data {dataIOs.ToString()}");

                            this.eventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(
                                    new NotificationMessage(
                                        msgData,
                                        "IO sensors status",
                                        MessageActor.Any,
                                        MessageActor.DeviceManager,
                                        MessageType.SensorsChanged,
                                        messageBayBayIndex,
                                        messageBayBayIndex,
                                        MessageStatus.OperationExecuting));

                            this.forceRemoteIoStatusPublish[ioIndex] = false;
                        }
                    }
                    break;

                case FieldMessageType.InverterStatusUpdate:

                    this.logger.LogTrace($"4:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData dataInverters)
                    {
                        var inverterIndex = receivedMessage.DeviceIndex;

                        if (dataInverters.CurrentPosition != null)
                        {
                            var notificationData = new PositioningMessageData();
                            var elevatorProvider = serviceProvider.GetRequiredService<IElevatorProvider>();
                            //TEMP Update X, Y axis positions
                            if (dataInverters.CurrentAxis == Axis.Vertical)
                            {
                                elevatorProvider.VerticalPosition = dataInverters.CurrentPosition.Value;
                                notificationData.AxisMovement = dataInverters.CurrentAxis;
                            }
                            else if (dataInverters.CurrentAxis == Axis.Horizontal)
                            {
                                elevatorProvider.HorizontalPosition = dataInverters.CurrentPosition.Value;
                                notificationData.AxisMovement = dataInverters.CurrentAxis;
                            }
                            else
                            {
                                var carouselProvider = serviceProvider.GetRequiredService<ICarouselProvider>();
                                carouselProvider.HorizontalPosition = dataInverters.CurrentPosition.Value;
                                notificationData.AxisMovement = Axis.Horizontal;
                                notificationData.MovementMode = MovementMode.BayChain;
                            }
                            this.logger.LogDebug($"InverterStatusUpdate inverter={inverterIndex}; axis={dataInverters.CurrentAxis}; value={(int)dataInverters.CurrentPosition.Value}");

                            this.currentStateMachines.TryGetValue(messageBayBayIndex, out var tempStateMachine);
                            if (tempStateMachine == null ||
                                tempStateMachine is InverterPowerEnableStateMachine ||
                                tempStateMachine is ResetFaultStateMachine ||
                                tempStateMachine is PowerEnableStateMachine)
                            {
                                notificationData.CurrentPosition = dataInverters.CurrentPosition.Value;
                                var notificationMessage = new NotificationMessage(
                                    notificationData,
                                    $"Current Encoder position: {notificationData.CurrentPosition}",
                                    MessageActor.AutomationService,
                                    MessageActor.DeviceManager,
                                    MessageType.Positioning,
                                    messageBayBayIndex,
                                    messageBayBayIndex,
                                    MessageStatus.OperationExecuting);

                                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                            }
                        }

                        if (machineResourcesProvider.UpdateInputs(inverterIndex, dataInverters.CurrentSensorStatus, receivedMessage.Source) || this.forceInverterIoStatusPublish)
                        {
                            var msgData = new SensorsChangedMessageData
                            {
                                SensorsStates = machineResourcesProvider.DisplayedInputs
                            };

                            var msg1 = new NotificationMessage(
                                msgData,
                                "IO sensors status",
                                MessageActor.Any,
                                MessageActor.DeviceManager,
                                MessageType.SensorsChanged,
                                messageBayBayIndex,
                                messageBayBayIndex,
                                MessageStatus.OperationExecuting);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg1);

                            this.forceInverterIoStatusPublish = false;
                        }
                    }
                    break;

                case FieldMessageType.InverterStatusWord:
                    this.logger.LogTrace($"5:InverterStatusWord received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is IInverterStatusWordFieldMessageData statusWordData)
                    {
                        var msgData = new InverterStatusWordMessageData(receivedMessage.DeviceIndex, statusWordData.Value);
                        var msg2 = new NotificationMessage(
                        msgData,
                        "Inverter Status Word",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterStatusWord,
                        messageBayBayIndex,
                        messageBayBayIndex,
                        MessageStatus.OperationExecuting);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg2);
                    }
                    break;

                // INFO Catch Exception from Inverter, to forward to the AS
                case FieldMessageType.InverterException:
                case FieldMessageType.InverterError:
                    var exceptionMessage = new InverterExceptionMessageData(null, receivedMessage.Description, 0);

                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage(
                                exceptionMessage,
                                "Inverter Exception",
                                MessageActor.Any,
                                MessageActor.DeviceManager,
                                MessageType.InverterException,
                                messageBayBayIndex,
                                messageBayBayIndex,
                                MessageStatus.OperationError,
                                ErrorLevel.Critical));

                    break;

                // INFO Catch Exception from IoDriver, to forward to the AS
                case FieldMessageType.IoDriverException:
                    var ioExceptionMessage = new IoDriverExceptionMessageData(null, receivedMessage.Description, 0);

                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage(
                                ioExceptionMessage,
                                "Io Driver Exception",
                                MessageActor.Any,
                                MessageActor.DeviceManager,
                                MessageType.IoDriverException,
                                messageBayBayIndex,
                                messageBayBayIndex,
                                MessageStatus.OperationError,
                                ErrorLevel.Critical));

                    break;

                case FieldMessageType.MeasureProfile:
                    messageBayBayIndex = BayNumber.ElevatorBay;
                    break;
            }

            this.currentStateMachines.TryGetValue(messageBayBayIndex, out var messageCurrentStateMachine);
            messageCurrentStateMachine?.ProcessFieldNotificationMessage(receivedMessage);
        }

        private void OnNotificationReceived(NotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var messageCurrentStateMachine);

            if (receivedMessage.Source == MessageActor.DeviceManager && receivedMessage.Destination == MessageActor.DeviceManager)
            {
                switch (receivedMessage.Type)
                {
                    case MessageType.Homing:
                    case MessageType.Positioning:
                    case MessageType.ShutterPositioning:
                    case MessageType.PowerEnable:
                    case MessageType.InverterFaultReset:
                    case MessageType.ResetSecurity:
                    case MessageType.InverterPowerEnable:
                        this.logger.LogTrace($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {receivedMessage.Status}");
                        this.currentStateMachines.Remove(receivedMessage.TargetBay);
                        this.SendCleanDebug();
                        break;
                }

                var notificationMessage = new NotificationMessage(
                    receivedMessage.Data,
                    receivedMessage.Description,
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    receivedMessage.Type,
                    receivedMessage.RequestingBay,
                    receivedMessage.TargetBay,
                    receivedMessage.Status,
                    receivedMessage.ErrorLevel);

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }

            switch (receivedMessage.Type)
            {
                case MessageType.DataLayerReady:

                    // TEMP Retrieve the current configuration of IO devices
                    this.RetrieveIoDevicesConfigurationAsync(serviceProvider);

                    var fieldNotification = new FieldNotificationMessage(
                        null,
                        "Data Layer Ready",
                        FieldMessageActor.Any,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.DataLayerReady,
                        MessageStatus.NoStatus,
                        (byte)InverterIndex.None);

                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(fieldNotification);

                    break;
            }

            messageCurrentStateMachine?.ProcessNotificationMessage(receivedMessage);
        }

        private void RetrieveIoDevicesConfigurationAsync(IServiceProvider serviceProvider)
        {
            var ioDevices = serviceProvider
                .GetRequiredService<IDigitalDevicesDataProvider>()
                .GetAllIoDevices();

            this.forceRemoteIoStatusPublish = new bool[ioDevices.Count()];

            var machineResourcesProvider = serviceProvider
                .GetRequiredService<IMachineResourcesProvider>();

            machineResourcesProvider.RunningStateChanged += this.MachineSensorsStatusOnRunningStateChanged;
            machineResourcesProvider.FaultStateChanged += this.MachineSensorsStatusOnFaultStateChanged;
        }

        private void SendCleanDebug()
        {
            {
                var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.DeviceManager, string.Empty, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current status null",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    MessageType.MachineStatusActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }

            {
                var notificationMessageData = new MachineStateActiveMessageData(MessageActor.DeviceManager, string.Empty, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current state null",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    MessageType.MachineStateActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }
        }

        private void SendNotificationMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "FSM Error",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.FsmException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        #endregion
    }
}
