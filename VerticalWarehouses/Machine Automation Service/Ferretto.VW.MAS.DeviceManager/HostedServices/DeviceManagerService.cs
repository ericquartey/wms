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
using Ferretto.VW.MAS.DeviceManager.CombinedMovements;
using Ferretto.VW.MAS.DeviceManager.CheckIntrusion;
using Ferretto.VW.MAS.DeviceManager.ExtBayPositioning;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable;
using Ferretto.VW.MAS.DeviceManager.Positioning;
using Ferretto.VW.MAS.DeviceManager.PowerEnable;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements;
using Ferretto.VW.MAS.DeviceManager.ResetFault;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning;
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
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal partial class DeviceManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly List<IStateMachine> currentStateMachines = new List<IStateMachine>();

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

        private readonly Task fieldNotificationReceiveTask;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly object syncObject = new object();

        private bool forceInverterIoStatusPublish;

        private bool[] forceRemoteIoStatusPublish;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public DeviceManagerService(
            IEventAggregator eventAggregator,
            ILogger<DeviceManagerService> logger,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));

            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));

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
            lock (this.syncObject)
            {
                var messageCurrentStateMachine = this.currentStateMachines.FirstOrDefault(c => c.BayNumber == command.TargetBay);

                var publishErrorNotification = false;
                if (messageCurrentStateMachine != null)
                {
                    if (messageCurrentStateMachine is RepetitiveHorizontalMovementsStateMachine ||
                        messageCurrentStateMachine is CombinedMovementsStateMachine)
                    {
                        publishErrorNotification = (command.Type != MessageType.Positioning
                            && command.Type != MessageType.Stop
                            && command.Type != MessageType.StopTest
                            && command.Type != MessageType.SensorsChanged
                            && command.Type != MessageType.PowerEnable
                            && command.Type != MessageType.ContinueMovement
                            && command.Type != MessageType.BayLight);
                    }
                    else
                    {
                        publishErrorNotification = (command.Type != MessageType.Stop
                            && command.Type != MessageType.StopTest
                            && command.Type != MessageType.SensorsChanged
                            && command.Type != MessageType.PowerEnable
                            && command.Type != MessageType.ContinueMovement
                            && command.Type != MessageType.BayLight
                            && command.Type != MessageType.CheckIntrusion);
                    }

                    // Publish a notification error, if occurs
                    if (publishErrorNotification)
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

                        var errorsProvider = serviceProvider.GetRequiredService<IErrorsProvider>();
                        errorsProvider.RecordNew(MachineErrorCode.BayInvertersBusy, command.RequestingBay);

                        return Task.CompletedTask;
                    }
                }

                // Process message and instantiate the related state machine
                this.Logger.LogDebug($"Processing command [{command.Type}] by {command.RequestingBay} for {command.TargetBay} from {command.Source}");
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

                    case MessageType.StopTest:
                        this.ProcessStopTest(command);
                        break;

                    case MessageType.ShutterPositioning:
                        this.ProcessShutterPositioningMessage(command, serviceProvider);
                        break;

                    case MessageType.Positioning when command.Data is IPositioningMessageData:
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

                    case MessageType.BayLight:
                        this.ProcessBayLight(command, serviceProvider);
                        break;

                    case MessageType.RepetitiveHorizontalMovements:
                        this.ProcessRepetitiveHorizontalMovements(command, serviceProvider);
                        break;

                    case MessageType.InverterProgramming:
                        this.ProcessInvertersProgramming(command, serviceProvider);
                        break;

                    case MessageType.InverterReading:
                        this.ProcessInvertersReading(command, serviceProvider);
                        break;

                    case MessageType.CheckIntrusion:
                        this.ProcessCheckIntrusion(command);
                        break;

                    case MessageType.CombinedMovements:
                        this.ProcessCombinedMovemets(command, serviceProvider);
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

                this.machineVolatileDataProvider.IsDeviceManagerBusy = this.currentStateMachines.Any();

                return Task.CompletedTask;
            }
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            lock (this.syncObject)
            {
                if (message.Source is MessageActor.DeviceManager
                    && message.Destination is MessageActor.DeviceManager
                    && this.currentStateMachines.Any(x => x.BayNumber == message.TargetBay)
                    )
                {
                    foreach (var messageCurrentStateMachine in this.currentStateMachines.Where(x => x.BayNumber == message.TargetBay).Reverse().ToList())
                    {
                        var notifyMessageToAny = true;

                        switch (message.Type)
                        {
                            case MessageType.Homing:
                            case MessageType.PowerEnable:
                            case MessageType.InverterFaultReset:
                            case MessageType.ResetSecurity:
                            case MessageType.InverterProgramming:
                            case MessageType.InverterReading:
                            case MessageType.InverterPowerEnable:
                            case MessageType.CheckIntrusion:
                                this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count}");
                                this.currentStateMachines.Remove(messageCurrentStateMachine);
                                this.SendCleanDebug();
                                break;

                            //case MessageType.Positioning:
                            //    if (!(messageCurrentStateMachine is PositioningStateMachine))
                            //    {
                            //        // deallocate only Positioning state machine
                            //        continue;
                            //    }
                            //    this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count()}");
                            //    this.currentStateMachines.Remove(messageCurrentStateMachine);
                            //    this.SendCleanDebug();
                            //    break;

                            // NEW block
                            case MessageType.Positioning:
                                if (messageCurrentStateMachine is PositioningStateMachine ||
                                    messageCurrentStateMachine is ExtBayPositioningStateMachine)
                                {
                                    if (messageCurrentStateMachine is PositioningStateMachine machine)
                                    {
                                        var positioningMessageData = message.Data as IPositioningMessageData;
                                        if (machine.AxisMovement == positioningMessageData.AxisMovement)
                                        {
                                            // deallocate only Positioning state machine
                                            this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count}");
                                            this.currentStateMachines.Remove(messageCurrentStateMachine);
                                            this.SendCleanDebug();
                                        }
                                        else
                                        {
                                            // Discharge the current state machine elaboration
                                            continue;
                                        }
                                    }

                                    if (messageCurrentStateMachine is ExtBayPositioningStateMachine)
                                    {
                                        // deallocate only Positioning state machine
                                        this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count}");
                                        this.currentStateMachines.Remove(messageCurrentStateMachine);
                                        this.SendCleanDebug();
                                    }
                                }
                                else
                                {
                                    if (!(messageCurrentStateMachine is RepetitiveHorizontalMovementsStateMachine) &&
                                        !(messageCurrentStateMachine is CombinedMovementsStateMachine))
                                    {
                                        continue;
                                    }

                                    if (messageCurrentStateMachine is CombinedMovementsStateMachine)
                                    {
                                        // Handle this exceptional case, where the message cannot be sent notification
                                        // to any destination (major on Machine Manager component)
                                        notifyMessageToAny = false;
                                    }

                                    // Current message must be processed by the RepetitiveHorizontalMovements state machine
                                    // or by the CombinedMovements state machine
                                }
                                break;

                            case MessageType.ShutterPositioning:
                                if (!(messageCurrentStateMachine is ShutterPositioningStateMachine))
                                {
                                    // deallocate only ShutterPositioning state machine
                                    continue;
                                }
                                this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count}");
                                this.currentStateMachines.Remove(messageCurrentStateMachine);
                                this.SendCleanDebug();
                                break;

                            case MessageType.RepetitiveHorizontalMovements:
                                if (!(messageCurrentStateMachine is RepetitiveHorizontalMovementsStateMachine))
                                {
                                    // deallocate only RepetitiveHorizontalMovements state machine
                                    continue;
                                }
                                this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count}");
                                this.currentStateMachines.Remove(messageCurrentStateMachine);
                                this.SendCleanDebug();
                                break;

                            case MessageType.CombinedMovements:
                                if (!(messageCurrentStateMachine is CombinedMovementsStateMachine))
                                {
                                    // deallocate only CombinedMovements state machine
                                    continue;
                                }
                                this.Logger.LogDebug($"16:Deallocation FSM [{messageCurrentStateMachine?.GetType().Name}] ended with {message.Status} count: {this.currentStateMachines.Count}");
                                this.currentStateMachines.Remove(messageCurrentStateMachine);
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

                        if (notifyMessageToAny)
                        {
                            // Publish notification message to any destination
                            this.EventAggregator
                                .GetEvent<NotificationEvent>()
                                .Publish(notificationMessage);
                        }

                        // Process message by the current state machine
                        messageCurrentStateMachine?.ProcessNotificationMessage(message);
                    }
                }

                if (message.Type is MessageType.DataLayerReady)
                {
                    this.Logger.LogTrace("OnDataLayerReady start");
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
                    this.Logger.LogTrace("OnDataLayerReady end");
                }
            }
            this.machineVolatileDataProvider.IsDeviceManagerBusy = this.currentStateMachines.Any();
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
                    this.Logger.LogDebug($"Emergency button status are [1:{this.machineResourcesProvider.IsMushroomEmergencyButtonBay1}, 2:{this.machineResourcesProvider.IsMushroomEmergencyButtonBay2}, 3:{this.machineResourcesProvider.IsMushroomEmergencyButtonBay3}]");
                    this.Logger.LogDebug($"Anti intrusion barrier status are [1:{this.machineResourcesProvider.IsAntiIntrusionBarrierBay1}, 2:{this.machineResourcesProvider.IsAntiIntrusionBarrierBay2}, 3:{this.machineResourcesProvider.IsAntiIntrusionBarrierBay3}]");
                    this.Logger.LogDebug($"Micro carter status are [Left:{this.machineResourcesProvider.IsMicroCarterLeftSide}, Right:{this.machineResourcesProvider.IsMicroCarterRightSide}]");

                    var errorCode = MachineErrorCode.SecurityWasTriggered;
                    if (this.machineResourcesProvider.IsMushroomEmergencyButtonBay1
                        || this.machineResourcesProvider.IsMushroomEmergencyButtonBay2
                        || this.machineResourcesProvider.IsMushroomEmergencyButtonBay3
                        )
                    {
                        errorCode = MachineErrorCode.SecurityButtonWasTriggered;
                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew(errorCode);
                    }
                    if (this.machineResourcesProvider.IsAntiIntrusionBarrierBay1
                        || this.machineResourcesProvider.IsAntiIntrusionBarrierBay2
                        || this.machineResourcesProvider.IsAntiIntrusionBarrierBay3
                        )
                    {
                        errorCode = MachineErrorCode.SecurityBarrierWasTriggered;
                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew(errorCode);
                    }
                    if (this.machineResourcesProvider.IsMicroCarterLeftSide)
                    {
                        errorCode = MachineErrorCode.SecurityLeftSensorWasTriggered;
                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew(errorCode);
                    }
                    if (this.machineResourcesProvider.IsMicroCarterRightSide)
                    {
                        errorCode = MachineErrorCode.SecurityRightSensorWasTriggered;
                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew(errorCode);
                    }
                    if (errorCode == MachineErrorCode.SecurityWasTriggered)
                    {
                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew(errorCode);
                    }
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

            if (receivedMessage.Status == MessageStatus.OperationUpdateData)
            {
                var s = string.Empty;
            }

            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();

            var bayNumber = BayNumber.None;
            switch (receivedMessage.Source)
            {
                case FieldMessageActor.IoDriver:
                    {
                        var messageIoIndex = Enum.Parse<IoIndex>(receivedMessage.DeviceIndex.ToString());
                        if (messageIoIndex != IoIndex.None)
                        {
                            bayNumber = baysDataProvider.GetByIoIndex(messageIoIndex, receivedMessage.Type);
                        }
                        break;
                    }

                case FieldMessageActor.InverterDriver:
                    {
                        var messageInverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
                        if (messageInverterIndex != InverterIndex.None)
                        {
                            bayNumber = baysDataProvider.GetByInverterIndex(messageInverterIndex, receivedMessage.Type);
                        }
                        break;
                    }
            }

            lock (this.syncObject)
            {
                switch (receivedMessage.Type)
                {
                    case FieldMessageType.CalibrateAxis:
                    case FieldMessageType.InverterPowerOff:
                        break;

                    case FieldMessageType.SensorsChanged when receivedMessage.Data is ISensorsChangedFieldMessageData:

                        this.Logger.LogTrace($"3:IOSensorsChanged received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}, data {receivedMessage.Data}");
                        var dataIOs = receivedMessage.Data as ISensorsChangedFieldMessageData;

                        var ioIndex = receivedMessage.DeviceIndex;
                        if (this.machineResourcesProvider.UpdateInputs(ioIndex, dataIOs.SensorsStates, receivedMessage.Source) || this.forceRemoteIoStatusPublish[ioIndex])
                        {
                            var msgData = new SensorsChangedMessageData
                            {
                                SensorsStates = this.machineResourcesProvider.DisplayedInputs
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

                        break;

                    case FieldMessageType.InverterStatusUpdate when receivedMessage.Data is IInverterStatusUpdateFieldMessageData:

                        this.Logger.LogTrace($"4:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");

                        var inverterData = receivedMessage.Data as IInverterStatusUpdateFieldMessageData;

                        if (inverterData.CurrentPosition != null)
                        {
                            var notificationData = new PositioningMessageData();
                            var elevatorProvider = serviceProvider.GetRequiredService<IElevatorProvider>();
                            var machineProvider = serviceProvider.GetRequiredService<IMachineVolatileDataProvider>();

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

                            var updatePosition = true;
                            if (this.currentStateMachines.Any(x => x.BayNumber == bayNumber))
                            {
                                foreach (var tempStateMachine in this.currentStateMachines.Where(x => x.BayNumber == bayNumber))
                                {
                                    if (!(tempStateMachine is InverterPowerEnableStateMachine)
                                        && !(tempStateMachine is ResetFaultStateMachine)
                                        && !(tempStateMachine is PowerEnableStateMachine)
                                        )
                                    {
                                        updatePosition = false;
                                        break;
                                    }
                                }
                            }

                            if (updatePosition)
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

                        if (this.machineResourcesProvider.UpdateInputs(inverterIndex, inverterData.CurrentSensorStatus, receivedMessage.Source) || this.forceInverterIoStatusPublish)
                        {
                            var msgData = new SensorsChangedMessageData
                            {
                                SensorsStates = this.machineResourcesProvider.DisplayedInputs
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

                        var args = new StatusUpdateEventArgs();
                        args.NewState = true;
                        this.machineResourcesProvider.OnFaultStateChanged(args);
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

                    case FieldMessageType.BayLight when receivedMessage.Source is FieldMessageActor.IoDriver &&
                                                        receivedMessage.Data is IBayLightFieldMessageData:

                        this.Logger.LogTrace($"3:BayLight received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}, data {receivedMessage.Data}");

                        var enable = ((IBayLightFieldMessageData)receivedMessage.Data).Enable;

                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            if (this.machineVolatileDataProvider.IsBayLightOn.ContainsKey(bayNumber))
                            {
                                this.machineVolatileDataProvider.IsBayLightOn[bayNumber] = enable;
                            }
                            else
                            {
                                this.machineVolatileDataProvider.IsBayLightOn.Add(bayNumber, enable);
                            }
                        }

                        this.EventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(
                                new NotificationMessage(
                                    null,
                                    $"BayLight={enable} completed, Bay={bayNumber}",
                                    MessageActor.Any,
                                    MessageActor.DeviceManager,
                                    MessageType.BayLight,
                                    bayNumber,
                                    bayNumber,
                                    receivedMessage.Status));

                        break;

                    case FieldMessageType.MeasureProfile:
                        if (!this.currentStateMachines.Any(x => x.BayNumber == bayNumber && x is CheckIntrusionStateMachine))
                        {
                            bayNumber = BayNumber.ElevatorBay;
                        }
                        break;

                    case FieldMessageType.Positioning when receivedMessage.Status is MessageStatus.OperationUpdateData &&
                                                           receivedMessage.Source is FieldMessageActor.InverterDriver &&
                                                           receivedMessage.Data is IInverterPositioningFieldMessageData:

                        var data = receivedMessage.Data as IInverterPositioningFieldMessageData;
                        var msg = new PositioningMessageData();

                        if (data != null)
                        {
                            msg.TorqueCurrentSample = new DataSample();
                            msg.TorqueCurrentSample.Value = data.AbsorbedCurrent;
                        }

                        this.EventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(
                                new NotificationMessage(
                                    msg,
                                    receivedMessage.Description,
                                    MessageActor.Any,
                                    MessageActor.DeviceManager,
                                    MessageType.Positioning,
                                    bayNumber,
                                    bayNumber,
                                    receivedMessage.Status));

                        break;

                    case FieldMessageType.InverterProgramming when receivedMessage.Source is FieldMessageActor.InverterDriver:

                        this.EventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(
                                new NotificationMessage(
                                    null,
                                    receivedMessage.Description,
                                    MessageActor.Any,
                                    MessageActor.DeviceManager,
                                    MessageType.InverterProgramming,
                                    bayNumber,
                                    bayNumber,
                                    receivedMessage.Status));

                        break;

                    case FieldMessageType.InverterReading when receivedMessage.Source is FieldMessageActor.InverterDriver:

                        this.EventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(
                                new NotificationMessage(
                                    null,
                                    receivedMessage.Description,
                                    MessageActor.Any,
                                    MessageActor.DeviceManager,
                                    MessageType.InverterReading,
                                    bayNumber,
                                    bayNumber,
                                    receivedMessage.Status));

                        break;
                }

                if (this.currentStateMachines.Any(x => x.BayNumber == bayNumber))
                {
                    foreach (var messageCurrentStateMachine in this.currentStateMachines.Where(x => x.BayNumber == bayNumber))
                    {
                        messageCurrentStateMachine.ProcessFieldNotificationMessage(receivedMessage);
                    }
                }
            }
        }

        private void RetrieveIoDevicesConfigurationAsync(IServiceProvider serviceProvider)
        {
            var ioDevices = serviceProvider
                .GetRequiredService<IDigitalDevicesDataProvider>()
                .GetAllIoDevices();

            this.forceRemoteIoStatusPublish = new bool[ioDevices.Count()];

            this.machineResourcesProvider.RunningStateChanged += this.MachineSensorsStatusOnRunningStateChanged;
            this.machineResourcesProvider.FaultStateChanged += this.MachineSensorsStatusOnFaultStateChanged;
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

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.Fail("Exception detected");
            }

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
