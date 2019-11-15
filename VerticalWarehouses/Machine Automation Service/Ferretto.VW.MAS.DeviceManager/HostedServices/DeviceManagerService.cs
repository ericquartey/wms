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
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable;
using Ferretto.VW.MAS.DeviceManager.PowerEnable;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ResetFault;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils;
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
    internal partial class DeviceManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly Dictionary<BayNumber, IStateMachine> currentStateMachines = new Dictionary<BayNumber, IStateMachine>();

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

        private readonly Task fieldNotificationReceiveTask;

        private bool forceInverterIoStatusPublish;

        private bool[] forceRemoteIoStatusPublish;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public DeviceManagerService(
            IEventAggregator eventAggregator,
            ILogger<DeviceManagerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.fieldNotificationReceiveTask = new Task(this.DequeueFieldNotifications);

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken).ConfigureAwait(true);

            this.stoppingToken = stoppingToken;

            try
            {
                this.fieldNotificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.SendCriticalErrorMessage(ex);
            }
        }

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination is MessageActor.DeviceManager
                ||
                command.Destination is MessageActor.Any;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination is MessageActor.DeviceManager
                ||
                notification.Destination is MessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            if (!this.currentStateMachines.TryGetValue(command.TargetBay, out var messageCurrentStateMachine))
            {
                messageCurrentStateMachine = null;
            }

            if (messageCurrentStateMachine != null
                && command.Type != MessageType.Stop
                && command.Type != MessageType.SensorsChanged
                && command.Type != MessageType.PowerEnable
                && command.Type != MessageType.ContinueMovement)
            {
                var errorNotification = new NotificationMessage(
                    command.Data,
                    $"Bay {command.RequestingBay} is already executing the machine {messageCurrentStateMachine.GetType().Name}",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    command.Type,
                    command.RequestingBay,
                    command.RequestingBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.Logger.LogWarning($"Bay {command.RequestingBay} is already executing the machine {messageCurrentStateMachine.GetType().Name}");
                this.Logger.LogError($"Message [{command.Type}] will be discarded!");

                this.EventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(errorNotification);

                return Task.CompletedTask;
            }

            this.Logger.LogInformation($"Processing command [{command.Type}] by {command.RequestingBay} for {command.TargetBay}");
            switch (command.Type)
            {
                case MessageType.ContinueMovement:
                    this.ProcessContinueMessage(command, serviceProvider);
                    break;

                case MessageType.Homing:
                    this.ProcessHomingMessage(command, serviceProvider);
                    break;

                case MessageType.Stop:
                    this.ProcessStopMessage(command);
                    break;

                case MessageType.ShutterPositioning:
                    this.ProcessShutterPositioningMessage(command, serviceProvider);
                    break;

                case MessageType.Positioning:
                    this.ProcessPositioningMessage(command, serviceProvider);
                    break;

                case MessageType.SensorsChanged:
                    this.ProcessSensorsChangedMessage(serviceProvider);
                    break;

                case MessageType.CheckCondition:
                    this.ProcessCheckConditionMessage(command, serviceProvider);
                    break;

                case MessageType.PowerEnable:
                    this.ProcessPowerEnableMessage(command, serviceProvider);
                    break;

                case MessageType.InverterStop:
                    this.ProcessInverterStopMessage();
                    break;

                case MessageType.InverterFaultReset:
                    this.ProcessInverterFaultResetMessage(command, serviceProvider);
                    break;

                case MessageType.ResetSecurity:
                    this.ProcessResetSecurityMessage(command);
                    break;

                case MessageType.InverterPowerEnable:
                    this.ProcessInverterPowerEnable(command, serviceProvider);
                    break;
            }

            var notificationMessageData = new MachineStatusActiveMessageData(
                MessageActor.DeviceManager,
                command.Type.ToString(),
                MessageVerbosity.Info);

            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"FSM current machine status {command.Type}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.MachineStatusActive,
                command.RequestingBay,
                command.RequestingBay,
                MessageStatus.OperationStart);

            messageCurrentStateMachine?.PublishNotificationMessage(notificationMessage);

            return Task.CompletedTask;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            this.currentStateMachines.TryGetValue(message.TargetBay, out var messageCurrentStateMachine);

            if (message.Source is MessageActor.DeviceManager
                &&
                message.Destination is MessageActor.DeviceManager)
            {
                switch (message.Type)
                {
                    case MessageType.Positioning:
                    case MessageType.Homing:
                    case MessageType.ShutterPositioning:
                    case MessageType.PowerEnable:
                    case MessageType.InverterFaultReset:
                    case MessageType.ResetSecurity:
                    case MessageType.InverterPowerEnable:
                        this.Logger.LogTrace($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status}");
                        this.currentStateMachines.Remove(message.TargetBay);
                        this.SendCleanDebug();
                        break;
                }

                var notificationMessage = new NotificationMessage(
                    message.Data,
                    message.Description,
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    message.Type,
                    message.RequestingBay,
                    message.TargetBay,
                    message.Status,
                    message.ErrorLevel);

                this.EventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }

            if (message.Type is MessageType.DataLayerReady)
            {
                // TEMP Retrieve the current configuration of IO devices
                this.RetrieveIoDevicesConfigurationAsync(serviceProvider);

                var fieldNotification = new FieldNotificationMessage(
                    null,
                    "Data Layer Ready",
                    FieldMessageActor.Any,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.DataLayerReady,
                    MessageStatus.NotSpecified,
                    (byte)InverterIndex.None);

                this.EventAggregator
                    .GetEvent<FieldNotificationEvent>()
                    .Publish(fieldNotification);
            }

            messageCurrentStateMachine?.ProcessNotificationMessage(message);

            return Task.CompletedTask;
        }

        private void DequeueFieldNotifications()
        {
            do
            {
                try
                {
                    if (this.fieldNotificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage))
                    {
                        this.Logger.LogTrace($"1:Queue Length({this.fieldNotificationQueue.Count}), Field Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");

                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            this.OnFieldNotificationReceived(receivedMessage, scope.ServiceProvider);
                        }
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogDebug($"Terminating field notifications thread for {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    this.SendCriticalErrorMessage(ex);
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void InitializeMethodSubscriptions()
        {
            var fieldNotificationEvent = this.EventAggregator.GetEvent<FieldNotificationEvent>();
            fieldNotificationEvent.Subscribe(
                message =>
                {
                    this.Logger.LogTrace($"Enqueue Field Notification message: {message.Type}, Source: {message.Source}, Destination {message.Destination}, Status: {message.Status}, Count: {this.fieldNotificationQueue.Count}");
                    this.fieldNotificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.DeviceManager || message.Destination == FieldMessageActor.Any);
        }

        private void MachineSensorsStatusOnFaultStateChanged(object sender, StatusUpdateEventArgs e)
        {
            if (e.NewState)
            {
                this.Logger.LogError($"Inverter Fault signal detected! Begin Stop machine procedure.");
                var messageData = new StateChangedMessageData(e.NewState);
                var msg = new NotificationMessage(
                    messageData,
                    "FSM Error",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    MessageType.FaultStateChanged,
                    BayNumber.None);
                this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                using (var scope = this.ServiceScopeFactory.CreateScope())
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

                        this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(commandMessage);
                    }
                }
            }
        }

        private void MachineSensorsStatusOnRunningStateChanged(object sender, StatusUpdateEventArgs e)
        {
            if (!e.NewState)
            {
                this.Logger.LogError($"Running State signal fall detected! Begin Stop machine procedure.");
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var machineResourcesProvider = scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
                    this.Logger.LogDebug($"Emergency button status are [1:{machineResourcesProvider.IsMushroomEmergencyButtonBay1}, 2:{machineResourcesProvider.IsMushroomEmergencyButtonBay2}, 3:{machineResourcesProvider.IsMushroomEmergencyButtonBay3}]");
                    this.Logger.LogDebug($"Anti intrusion barrier status are [1:{machineResourcesProvider.IsAntiIntrusionBarrierBay1}, 2:{machineResourcesProvider.IsAntiIntrusionBarrierBay2}, 3:{machineResourcesProvider.IsAntiIntrusionBarrierBay3}]");
                    this.Logger.LogDebug($"Micro carter status are [Left:{machineResourcesProvider.IsMicroCarterLeftSide}, Right:{machineResourcesProvider.IsMicroCarterRightSide}]");
                }
            }

            var messageData = new StateChangedMessageData(e.NewState);
            var msg = new NotificationMessage(
                messageData,
                "FSM Error",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.RunningStateChanged,
                BayNumber.None);
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
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

            var baysDataProvider = serviceProvider.GetRequiredService<IBaysProvider>();

            BayNumber bayNumber;
            switch (receivedMessage.Source)
            {
                case FieldMessageActor.IoDriver:
                    {
                        var messageIoIndex = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());
                        bayNumber = baysDataProvider.GetByIoIndex(messageIoIndex, receivedMessage.Type);
                        break;
                    }

                case FieldMessageActor.InverterDriver:
                    {
                        var messageInverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
                        bayNumber = baysDataProvider.GetByInverterIndex(messageInverterIndex);
                        break;
                    }

                default:
                    {
                        bayNumber = BayNumber.None;
                        break;
                    }
            }

            var machineResourcesProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();
            switch (receivedMessage.Type)
            {
                case FieldMessageType.CalibrateAxis:
                case FieldMessageType.InverterPowerOff:
                    break;

                case FieldMessageType.SensorsChanged:

                    this.Logger.LogTrace($"3:IOSensorsChanged received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}, data {receivedMessage.Data}");
                    if (receivedMessage.Data is ISensorsChangedFieldMessageData dataIOs)
                    {
                        var ioIndex = receivedMessage.DeviceIndex;
                        if (machineResourcesProvider.UpdateInputs(ioIndex, dataIOs.SensorsStates, receivedMessage.Source) || this.forceRemoteIoStatusPublish[ioIndex])
                        {
                            var msgData = new SensorsChangedMessageData
                            {
                                SensorsStates = machineResourcesProvider.DisplayedInputs
                            };

                            this.Logger.LogTrace($"FSM: IoIndex {ioIndex}, data {dataIOs.ToString()}");

                            this.EventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(
                                    new NotificationMessage(
                                        msgData,
                                        "IO sensors status",
                                        MessageActor.Any,
                                        MessageActor.DeviceManager,
                                        MessageType.SensorsChanged,
                                        bayNumber,
                                        bayNumber,
                                        MessageStatus.OperationExecuting));

                            this.forceRemoteIoStatusPublish[ioIndex] = false;
                        }
                    }
                    break;

                case FieldMessageType.InverterStatusUpdate:

                    this.Logger.LogTrace($"4:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");

                    System.Diagnostics.Debug.Assert(receivedMessage.Data is IInverterStatusUpdateFieldMessageData);

                    var inverterData = receivedMessage.Data as IInverterStatusUpdateFieldMessageData;

                    if (inverterData.CurrentPosition != null)
                    {
                        var notificationData = new PositioningMessageData();
                        var elevatorProvider = serviceProvider.GetRequiredService<IElevatorProvider>();

                        // TEMP Update X, Y axis positions
                        if (inverterData.CurrentAxis is Axis.Vertical)
                        {
                            elevatorProvider.VerticalPosition = inverterData.CurrentPosition.Value;
                            notificationData.AxisMovement = inverterData.CurrentAxis;
                        }
                        else if (inverterData.CurrentAxis is Axis.Horizontal)
                        {
                            elevatorProvider.HorizontalPosition = inverterData.CurrentPosition.Value;
                            notificationData.AxisMovement = inverterData.CurrentAxis;
                        }
                        else
                        {
                            baysDataProvider.SetChainPosition(bayNumber, inverterData.CurrentPosition.Value);

                            notificationData.AxisMovement = Axis.BayChain;
                            notificationData.MovementMode = MovementMode.BayChain;
                        }

                        this.Logger.LogDebug($"InverterStatusUpdate inverter={receivedMessage.DeviceIndex}; Movement={notificationData.AxisMovement}; value={inverterData.CurrentPosition.Value:0.0000}");

                        this.currentStateMachines.TryGetValue(bayNumber, out var tempStateMachine);
                        if (tempStateMachine is null ||
                            tempStateMachine is InverterPowerEnableStateMachine ||
                            tempStateMachine is ResetFaultStateMachine ||
                            tempStateMachine is PowerEnableStateMachine)
                        {
                            var notificationMessage = new NotificationMessage(
                                notificationData,
                                $"Current Encoder position updated",
                                MessageActor.AutomationService,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                bayNumber,
                                bayNumber,
                                MessageStatus.OperationExecuting);

                            this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                        }
                    }

                    var inverterIndex = receivedMessage.DeviceIndex;

                    if (machineResourcesProvider.UpdateInputs(inverterIndex, inverterData.CurrentSensorStatus, receivedMessage.Source) || this.forceInverterIoStatusPublish)
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
                            bayNumber,
                            bayNumber,
                            MessageStatus.OperationExecuting);
                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg1);

                        this.forceInverterIoStatusPublish = false;
                    }

                    break;

                case FieldMessageType.InverterStatusWord:
                    this.Logger.LogTrace($"5:InverterStatusWord received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is IInverterStatusWordFieldMessageData statusWordData)
                    {
                        var msgData = new InverterStatusWordMessageData(receivedMessage.DeviceIndex, statusWordData.Value);
                        var msg2 = new NotificationMessage(
                        msgData,
                        "Inverter Status Word",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterStatusWord,
                        bayNumber,
                        bayNumber,
                        MessageStatus.OperationExecuting);
                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg2);
                    }
                    break;

                // INFO Catch Exception from Inverter, to forward to the AS
                case FieldMessageType.InverterException:
                case FieldMessageType.InverterError:
                    var exceptionMessage = new InverterExceptionMessageData(null, receivedMessage.Description, 0);

                    this.EventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage(
                                exceptionMessage,
                                "Inverter Exception",
                                MessageActor.Any,
                                MessageActor.DeviceManager,
                                MessageType.InverterException,
                                bayNumber,
                                bayNumber,
                                MessageStatus.OperationError,
                                receivedMessage.ErrorLevel));

                    break;

                // INFO Catch Exception from IoDriver, to forward to the AS
                case FieldMessageType.IoDriverException:
                    var ioExceptionMessage = new IoDriverExceptionMessageData(null, receivedMessage.Description, 0);

                    this.EventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage(
                                ioExceptionMessage,
                                "Io Driver Exception",
                                MessageActor.Any,
                                MessageActor.DeviceManager,
                                MessageType.IoDriverException,
                                bayNumber,
                                bayNumber,
                                MessageStatus.OperationError,
                                receivedMessage.ErrorLevel));

                    break;

                case FieldMessageType.MeasureProfile:
                    bayNumber = BayNumber.ElevatorBay;
                    break;
            }

            this.currentStateMachines.TryGetValue(bayNumber, out var messageCurrentStateMachine);
            messageCurrentStateMachine?.ProcessFieldNotificationMessage(receivedMessage);
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

                this.EventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }

            {
                var notificationMessageData = new MachineStateActiveMessageData(
                    MessageActor.DeviceManager,
                    string.Empty,
                    MessageVerbosity.Info);

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current state null",
                    MessageActor.Any,
                    MessageActor.DeviceManager,
                    MessageType.MachineStateActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.EventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }
        }

        private void SendCriticalErrorMessage(Exception ex)
        {
            this.SendCriticalErrorMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
        }

        private void SendCriticalErrorMessage(IFsmExceptionMessageData data)
        {
            this.Logger.LogCritical($"Exception detected: {data.ExceptionDescription} {data.InnerException?.Message}");
            if (data.InnerException != null)
            {
                this.Logger.LogError(data.InnerException, data.InnerException.Message);
            }

            System.Diagnostics.Debug.Fail("Exception detected");

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage(
                        data,
                        "FSM Error",
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.FsmException,
                        BayNumber.None,
                        BayNumber.None,
                        MessageStatus.OperationError,
                        ErrorLevel.Error));
        }

        #endregion
    }
}
