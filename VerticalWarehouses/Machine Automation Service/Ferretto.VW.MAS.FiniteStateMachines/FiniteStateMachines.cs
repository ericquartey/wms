using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.FiniteStateMachines
{
    internal partial class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue = new BlockingConcurrentQueue<CommandMessage>();

        private readonly Task commandReceiveTask;

        private readonly Dictionary<BayNumber, IStateMachine> currentStateMachines = new Dictionary<BayNumber, IStateMachine>();

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

        private readonly Task fieldNotificationReceiveTask;

        private readonly ILogger<FiniteStateMachines> logger;

        private readonly IMachineProvider machineProvider;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

        private readonly Task notificationReceiveTask;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly ISetupStatusProvider setupStatusProvider;

        private bool forceInverterIoStatusPublish;

        private bool forceRemoteIoStatusPublish;

        private IEnumerable<IoDevice> ioDevices;

        private bool isDisposed;

        private MachineSensorsStatus machineSensorsStatus;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public FiniteStateMachines(
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            ISetupStatusProvider setupStatusProvider,
            IMachineProvider machineProvider,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IBaysProvider baysProvider,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.digitalDevicesDataProvider = digitalDevicesDataProvider;
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.commandReceiveTask = new Task(this.CommandReceiveTaskFunction);
            this.notificationReceiveTask = new Task(this.NotificationReceiveTaskFunction);
            this.fieldNotificationReceiveTask = new Task(this.FieldNotificationReceiveTaskFunction);

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Destructors

        ~FiniteStateMachines()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.isDisposed = true;
        }

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
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, TargetBay:{receivedMessage.TargetBay}");
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

                if (!this.currentStateMachines.TryGetValue(receivedMessage.TargetBay,
                    out var messageCurrentStateMachine))
                {
                    messageCurrentStateMachine = null;
                }

                if (messageCurrentStateMachine != null
                    && receivedMessage.Type != MessageType.Stop
                    && receivedMessage.Type != MessageType.SensorsChanged
                    && receivedMessage.Type != MessageType.PowerEnable
                    && receivedMessage.Type != MessageType.RequestPosition
                    )
                {
                    var errorNotification = new NotificationMessage(
                        receivedMessage.Data,
                        $"Bay {receivedMessage.RequestingBay} is already executing the machine {messageCurrentStateMachine.GetType()}",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        receivedMessage.Type,
                        receivedMessage.RequestingBay,
                        receivedMessage.RequestingBay,
                        MessageStatus.OperationError,
                        ErrorLevel.Error);

                    this.logger.LogWarning($"Bay {receivedMessage.RequestingBay} is already executing the machine {messageCurrentStateMachine.GetType()}");
                    this.logger.LogError($"Message [{receivedMessage.Type}] will be discarded!");

                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
                    continue;
                }

                this.logger.LogInformation($"Processing command [{receivedMessage.Type}] by {receivedMessage.RequestingBay} for {receivedMessage.TargetBay}");
                switch (receivedMessage.Type)
                {
                    case MessageType.Homing:
                        this.ProcessHomingMessage(receivedMessage);
                        break;

                    case MessageType.Stop:
                        this.ProcessStopMessage(receivedMessage);
                        break;

                    case MessageType.ShutterPositioning:
                        this.ProcessShutterPositioningMessage(receivedMessage);
                        break;

                    case MessageType.Positioning:
                        this.ProcessPositioningMessage(receivedMessage);
                        break;

                    case MessageType.SensorsChanged:
                        this.ProcessSensorsChangedMessage();
                        break;

                    case MessageType.CheckCondition:
                        this.ProcessCheckConditionMessage(receivedMessage);
                        break;

                    case MessageType.DrawerOperation:
                        this.ProcessDrawerOperation(receivedMessage);
                        break;

                    case MessageType.PowerEnable:
                        this.ProcessPowerEnableMessage(receivedMessage);
                        break;

                    case MessageType.InverterStop:
                        this.ProcessInverterStopMessage();
                        break;

                    case MessageType.RequestPosition:
                        this.ProcessRequestPositionMessage(receivedMessage);
                        break;

                    case MessageType.InverterFaultReset:
                        this.ProcessInverterFaultResetMessage(receivedMessage);
                        break;

                    case MessageType.ResetSecurity:
                        this.ProcessResetSecurityMessage(receivedMessage);
                        break;
                }

                var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.FiniteStateMachines, receivedMessage.Type.ToString(), MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current machine status {receivedMessage.Type}",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.MachineStatusActive,
                    receivedMessage.RequestingBay,
                    receivedMessage.RequestingBay,
                    MessageStatus.OperationStart);

                messageCurrentStateMachine?.PublishNotificationMessage(notificationMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void FieldNotificationReceiveTaskFunction()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var scopedBaysProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();
                do
                {
                    FieldNotificationMessage receivedMessage;
                    try
                    {
                        this.fieldNotificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                        this.logger.LogTrace($"1:Queue Length({this.fieldNotificationQueue.Count}), Field Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
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

                    var messageBayBayIndex = BayNumber.None;
                    if (receivedMessage.Source is FieldMessageActor.IoDriver)
                    {
                        var messageIoIndex = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());
                        messageBayBayIndex = scopedBaysProvider.GetByIoIndex(messageIoIndex, receivedMessage.Type);
                    }

                    if (receivedMessage.Source is FieldMessageActor.InverterDriver)
                    {
                        var messageInverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
                        messageBayBayIndex = scopedBaysProvider.GetByInverterIndex(messageInverterIndex);
                    }

                    this.currentStateMachines.TryGetValue(messageBayBayIndex, out var messageCurrentStateMachine);

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

                                if (this.machineSensorsStatus.UpdateInputs(ioIndex, dataIOs.SensorsStates, receivedMessage.Source) || this.forceRemoteIoStatusPublish)
                                {
                                    var msgData = new SensorsChangedMessageData();
                                    msgData.SensorsStates = this.machineSensorsStatus.DisplayedInputs;
                                    this.logger.LogTrace($"FSM: IoIndex {ioIndex}, data {dataIOs.ToString()}");

                                    var msg = new NotificationMessage(
                                        msgData,
                                        "IO sensors status",
                                        MessageActor.Any,
                                        MessageActor.FiniteStateMachines,
                                        MessageType.SensorsChanged,
                                        messageBayBayIndex,
                                        messageBayBayIndex,
                                        MessageStatus.OperationExecuting);

                                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(msg);

                                    this.forceRemoteIoStatusPublish = false;
                                }
                            }
                            break;

                        case FieldMessageType.InverterStatusUpdate:

                            this.logger.LogTrace($"4:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                            if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData dataInverters)
                            {
                                var inverterIndex = receivedMessage.DeviceIndex;

                                //TEMP Update X, Y axis positions
                                if (dataInverters.CurrentAxis == Axis.Vertical)
                                {
                                    lock (this.machineSensorsStatus)
                                    {
                                        this.machineSensorsStatus.AxisYPosition = dataInverters.CurrentPosition.Value;
                                    }
                                }
                                else if (dataInverters.CurrentAxis == Axis.Horizontal)
                                {
                                    lock (this.machineSensorsStatus)
                                    {
                                        this.machineSensorsStatus.AxisXPosition = dataInverters.CurrentPosition.Value;
                                    }
                                }

                                if (this.machineSensorsStatus.UpdateInputs(inverterIndex, dataInverters.CurrentSensorStatus, receivedMessage.Source) || this.forceInverterIoStatusPublish)
                                {
                                    var msgData = new SensorsChangedMessageData
                                    {
                                        SensorsStates = this.machineSensorsStatus.DisplayedInputs
                                    };

                                    var msg1 = new NotificationMessage(
                                        msgData,
                                        "IO sensors status",
                                        MessageActor.Any,
                                        MessageActor.FiniteStateMachines,
                                        MessageType.SensorsChanged,
                                        messageBayBayIndex,
                                        messageBayBayIndex,
                                        MessageStatus.OperationExecuting);
                                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(msg1);

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
                                MessageActor.FiniteStateMachines,
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

                            var msg4 = new NotificationMessage(
                                exceptionMessage,
                                "Inverter Exception",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.InverterException,
                                messageBayBayIndex,
                                messageBayBayIndex,
                                MessageStatus.OperationError,
                                ErrorLevel.Critical);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg4);

                            break;

                        // INFO Catch Exception from IoDriver, to forward to the AS
                        case FieldMessageType.IoDriverException:
                            var ioExceptionMessage = new IoDriverExceptionMessageData(null, receivedMessage.Description, 0);

                            var msg5 = new NotificationMessage(
                                ioExceptionMessage,
                                "Io Driver Exception",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.IoDriverException,
                                messageBayBayIndex,
                                messageBayBayIndex,
                                MessageStatus.OperationError,
                                ErrorLevel.Critical);
                            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(msg5);

                            break;
                    }
                    messageCurrentStateMachine?.ProcessFieldNotificationMessage(receivedMessage);
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
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                message =>
                {
                    this.logger.LogTrace($"Enqueue Notification message: {message.Type}, Source: {message.Source}, Destination {message.Destination}, Status: {message.Status}");
                    this.notificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            var fieldNotificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            fieldNotificationEvent.Subscribe(
                message =>
                {
                    this.logger.LogTrace($"Enqueue Field Notification message: {message.Type}, Source: {message.Source}, Destination {message.Destination}, Status: {message.Status}, Count: {this.fieldNotificationQueue.Count}");
                    this.fieldNotificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.FiniteStateMachines || message.Destination == FieldMessageActor.Any);
        }

        private void MachineSensorsStatusOnFaultStateChanged(object sender, StatusUpdateEventArgs e)
        {
            var messageData = new StateChangedMessageData(e.NewState);
            var msg = new NotificationMessage(
                messageData,
                "FSM Error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.FaultStateChanged,
                BayNumber.None);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        private void MachineSensorsStatusOnRunningStateChanged(object sender, StatusUpdateEventArgs e)
        {
            var messageData = new StateChangedMessageData(e.NewState);
            var msg = new NotificationMessage(
                messageData,
                "FSM Error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.RunningStateChanged,
                BayNumber.None);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        private void NotificationReceiveTaskFunction()
        {
            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Queue Length ({this.notificationQueue.Count}), Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status} TargetBay:{receivedMessage.TargetBay}");
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

                this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var messageCurrentStateMachine);

                if (receivedMessage.Source == MessageActor.FiniteStateMachines && receivedMessage.Destination == MessageActor.FiniteStateMachines)
                {
                    switch (receivedMessage.Type)
                    {
                        case MessageType.Homing:
                            if (receivedMessage.Status == MessageStatus.OperationEnd)
                            {
                                try
                                {
                                    this.setupStatusProvider.CompleteVerticalOrigin();
                                }
                                catch (Exception ex)
                                {
                                    this.logger.LogDebug($"4:Exception: {ex.Message}");

                                    this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                                }
                            }
                            this.logger.LogTrace($"16:Deallocation FSM {messageCurrentStateMachine?.GetType()} ended with {receivedMessage.Status}");
                            this.currentStateMachines.Remove(receivedMessage.TargetBay);
                            this.SendCleanDebug();
                            break;

                        case MessageType.Positioning:
                        case MessageType.ShutterPositioning:
                        case MessageType.DrawerOperation:
                        case MessageType.PowerEnable:
                        case MessageType.InverterFaultReset:
                        case MessageType.ResetSecurity:
                            this.logger.LogTrace($"16:Deallocation FSM {messageCurrentStateMachine?.GetType()} ended with {receivedMessage.Status}");
                            this.currentStateMachines.Remove(receivedMessage.TargetBay);
                            this.SendCleanDebug();
                            break;
                    }

                    var notificationMessage = new NotificationMessage(
                        receivedMessage.Data,
                        receivedMessage.Description,
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        receivedMessage.Type,
                        receivedMessage.RequestingBay,
                        receivedMessage.TargetBay,
                        receivedMessage.Status,
                        receivedMessage.ErrorLevel);

                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.DataLayerReady:

                        // TEMP Retrieve the current configuration of IO devices
                        this.RetrieveIoDevicesConfigurationAsync();

                        var fieldNotification = new FieldNotificationMessage(
                            null,
                            "Data Layer Ready",
                            FieldMessageActor.Any,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.DataLayerReady,
                            MessageStatus.NoStatus,
                            (byte)InverterIndex.None);

                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(fieldNotification);

                        break;
                }

                messageCurrentStateMachine?.ProcessNotificationMessage(receivedMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void RetrieveIoDevicesConfigurationAsync()
        {
            this.ioDevices = this.digitalDevicesDataProvider.GetAllIoDevices();

            this.machineSensorsStatus = new MachineSensorsStatus(this.machineProvider.IsOneTonMachine());

            this.machineSensorsStatus.RunningStateChanged += this.MachineSensorsStatusOnRunningStateChanged;
            this.machineSensorsStatus.FaultStateChanged += this.MachineSensorsStatusOnFaultStateChanged;
        }

        private void SendCleanDebug()
        {
            {
                var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.FiniteStateMachines, string.Empty, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current status null",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.MachineStatusActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }

            {
                var notificationMessageData = new MachineStateActiveMessageData(MessageActor.FiniteStateMachines, string.Empty, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current state null",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.MachineStateActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }
        }

        private void SendNotificationMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "FSM Error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
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
