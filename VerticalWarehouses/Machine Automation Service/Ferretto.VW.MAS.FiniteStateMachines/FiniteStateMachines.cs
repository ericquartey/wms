using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus;
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
namespace Ferretto.VW.MAS.FiniteStateMachines
{
    internal partial class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private const int STATUS_WORD_UPDATE_INTERVAL = 600;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue;

        private readonly Task fieldNotificationReceiveTask;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoDataLayer;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly ILogger<FiniteStateMachines> logger;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private readonly IVertimagConfigurationDataLayer vertimagConfiguration;

        private IStateMachine currentStateMachine;

        private Timer delayTimer;

        private bool forceInverterIoStatusPublish;

        private bool forceRemoteIoStatusPublish;

        private List<IoIndex> ioIndexDeviceList;

        private bool isDataLayerReady;

        private bool isDisposed;

        private MachineSensorsStatus machineSensorsStatus;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public FiniteStateMachines(
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            ISetupStatusProvider setupStatusProvider,
            IVertimagConfigurationDataLayer vertimagConfiguration,
            IGeneralInfoConfigurationDataLayer generalInfoDataLayer,
            IVerticalAxisDataLayer verticalAxis,
            IHorizontalAxisDataLayer horizontalAxis,
            IMachineConfigurationProvider machineConfigurationProvider,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (setupStatusProvider == null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            if (machineConfigurationProvider is null)
            {
                throw new ArgumentNullException(nameof(machineConfigurationProvider));
            }

            if (serviceScopeFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;

            this.setupStatusProvider = setupStatusProvider;

            this.vertimagConfiguration = vertimagConfiguration;

            this.generalInfoDataLayer = generalInfoDataLayer;

            this.verticalAxis = verticalAxis;

            this.horizontalAxis = horizontalAxis;
            this.machineConfigurationProvider = machineConfigurationProvider;

            this.serviceScopeFactory = serviceScopeFactory;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.fieldNotificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task(this.CommandReceiveTaskFunction);
            this.notificationReceiveTask = new Task(this.NotificationReceiveTaskFunction);
            this.fieldNotificationReceiveTask = new Task(this.FieldNotificationReceiveTaskFunction);

            this.logger.LogTrace("1:Subscription Command");

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();

            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
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

                this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
            }

            await Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            this.delayTimer?.Dispose();
            this.delayTimer = new Timer(this.DelayTimerMethod, null, -1, Timeout.Infinite);

            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"2:Exception: {ex.Message}");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                    return;
                }

                if (this.currentStateMachine != null
                    && receivedMessage.Type != MessageType.Stop
                    && receivedMessage.Type != MessageType.SensorsChanged
                    && receivedMessage.Type != MessageType.PowerEnable
                    && receivedMessage.Type != MessageType.RequestPosition
                    )
                {
                    var errorNotification = new NotificationMessage(
                        null,
                        "Inverter operation already in progress",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        receivedMessage.Type,
                        MessageStatus.OperationError,
                        ErrorLevel.Error);

                    this.logger.LogWarning($"3:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
                    continue;
                }

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
                    case MessageType.TorqueCurrentSampling:
                        this.ProcessPositioningMessage(receivedMessage?.Data as IPositioningMessageData);
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

                    case MessageType.ResetSecurity:
                        this.ProcessResetSecurityMessage();
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
                }

                var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.FiniteStateMachines, receivedMessage.Type.ToString(), MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"FSM current machine status {receivedMessage.Type}",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.MachineStatusActive,
                    MessageStatus.OperationStart);

                this.currentStateMachine?.PublishNotificationMessage(notificationMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void FieldNotificationReceiveTaskFunction()
        {
            do
            {
                try
                {
                    this.fieldNotificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage);

                    this.logger.LogTrace($"1:Queue Length({this.fieldNotificationQueue.Count}), Field Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");

                    this.OnFieldNotificationMessageReceived(receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"2:Exception: {ex.Message}");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                    return;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void InitializeMethodSubscriptions()
        {
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

        private void NotificationReceiveTaskFunction()
        {
            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Queue Length ({this.notificationQueue.Count}), Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message}");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                    return;
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

                    case MessageType.Homing:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            switch (receivedMessage.Status)
                            {
                                case MessageStatus.OperationEnd:
                                    try
                                    {
                                        this.setupStatusProvider.CompleteVerticalOrigin();
                                    }
                                    catch (Exception ex)
                                    {
                                        this.logger.LogDebug($"4:Exception: {ex.Message}");

                                        this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                                    }

                                    this.logger.LogTrace($"5:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"6:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"7:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();

                                    //TODO: According to the type of error we can try to resolve here
                                    break;
                            }
                        }
                        break;

                    case MessageType.Positioning:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            switch (receivedMessage.Status)
                            {
                                case MessageStatus.OperationEnd:

                                    this.logger.LogTrace($"8:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"9:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"10:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();

                                    //TODO: According to the type of error we can try to resolve here
                                    break;
                            }
                        }
                        break;

                    case MessageType.ShutterPositioning:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            switch (receivedMessage.Status)
                            {
                                case MessageStatus.OperationEnd:

                                    this.logger.LogTrace($"11:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"12:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"13:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();

                                    //TODO: According to the type of error we can try to resolve here
                                    break;
                            }
                        }
                        break;

                    case MessageType.DrawerOperation:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            switch (receivedMessage.Status)
                            {
                                case MessageStatus.OperationEnd:

                                    this.logger.LogDebug($"17:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"18:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;

                    case MessageType.ResetSecurity:
                    case MessageType.PowerEnable:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            switch (receivedMessage.Status)
                            {
                                case MessageStatus.OperationEnd:

                                    this.logger.LogTrace($"14:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"15:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();
                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"16:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;
                                    this.SendCleanDebug();

                                    //TODO: According to the type of error we can try to resolve here
                                    break;
                            }
                        }
                        break;

                    // TODO temporary message to show error on UI
                    case MessageType.InverterException:
                        var exceptionMessage = new InverterExceptionMessageData(null, receivedMessage.Description, 0);

                        var msg = new NotificationMessage(
                            exceptionMessage,
                            "Inverter Exception",
                            MessageActor.WebApi,
                            MessageActor.FiniteStateMachines,
                            MessageType.InverterException,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                        break;
                }

                this.currentStateMachine?.ProcessNotificationMessage(receivedMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void OnFieldNotificationMessageReceived(FieldNotificationMessage receivedMessage)
        {
            switch (receivedMessage.Type)
            {
                case FieldMessageType.CalibrateAxis:
                case FieldMessageType.InverterPowerOff:
                    break;

                case FieldMessageType.SensorsChanged:

                    if (!this.isDataLayerReady)
                    {
                        this.logger.LogWarning(
                            $"Field notification message {FieldMessageType.SensorsChanged} was discarded, because data layer is not yet ready.");
                        return;
                    }

                    this.logger.LogTrace($"3:IOSensorsChanged received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is ISensorsChangedFieldMessageData dataIOs)
                    {
                        var ioIndex = receivedMessage.DeviceIndex;
                        var oldNormalState = this.machineSensorsStatus.IsMachineInNormalState;

                        if (this.machineSensorsStatus.UpdateInputs(ioIndex, dataIOs.SensorsStates, receivedMessage.Source) || this.forceRemoteIoStatusPublish)
                        {
                            var msgData = new SensorsChangedMessageData();
                            msgData.SensorsStates = this.machineSensorsStatus.DisplayedInputs;

                            var msg = new NotificationMessage(
                                msgData,
                                "IO sensors status",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.SensorsChanged,
                                MessageStatus.OperationExecuting);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                            this.forceRemoteIoStatusPublish = false;
                        }

                        if (oldNormalState
                            && (!this.machineSensorsStatus.IsMachineInNormalState || this.machineSensorsStatus.IsInverterInFault)
                            && (this.currentStateMachine == null || !this.currentStateMachine.GetType().ToString().Contains("PowerEnableStateMachine"))
                            )
                        {
                            if (this.machineSensorsStatus.IsInverterInFault)
                            {
                                this.logger.LogWarning($"3b:Inverter fault detected! Set Power Enable Off.");
                            }
                            else
                            {
                                this.logger.LogWarning($"3b:Normal machine state fall detected! Set Power Enable Off.");
                            }
                            var powerEnableData = new PowerEnableMessageData(false);
                            this.CreatePowerEnableStateMachine(powerEnableData);
                        }
                    }
                    break;

                case FieldMessageType.InverterStatusUpdate:

                    if (!this.isDataLayerReady)
                    {
                        this.logger.LogWarning(
                            $"Field notification message {FieldMessageType.SensorsChanged} was discarded, because data layer is not yet ready.");
                        return;
                    }

                    this.logger.LogTrace($"4:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData dataInverters)
                    {
                        var inverterIndex = receivedMessage.DeviceIndex;

                        //TEMP Update X, Y axis positions
                        if (dataInverters.CurrentAxis == Axis.Vertical)
                        {
                            lock (this.machineSensorsStatus)
                            {
                                this.machineSensorsStatus.AxisYPosition = dataInverters.CurrentPosition;
                            }
                        }
                        else if (dataInverters.CurrentAxis == Axis.Horizontal)
                        {
                            lock (this.machineSensorsStatus)
                            {
                                this.machineSensorsStatus.AxisXPosition = dataInverters.CurrentPosition;
                            }
                        }

                        if (this.machineSensorsStatus.UpdateInputs(inverterIndex, dataInverters.CurrentSensorStatus, receivedMessage.Source) || this.forceInverterIoStatusPublish)
                        {
                            var msgData = new SensorsChangedMessageData();
                            msgData.SensorsStates = this.machineSensorsStatus.DisplayedInputs;

                            var msg = new NotificationMessage(
                                msgData,
                                "IO sensors status",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.SensorsChanged,
                                MessageStatus.OperationExecuting);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                            this.forceInverterIoStatusPublish = false;

                            //if(this.machineSensorsStatus.IsInverterFault)
                        }
                    }
                    break;

                case FieldMessageType.InverterStatusWord:

                    if (!this.isDataLayerReady)
                    {
                        this.logger.LogWarning(
                            $"Field notification message {FieldMessageType.SensorsChanged} was discarded, because data layer is not yet ready.");
                        return;
                    }

                    this.logger.LogTrace($"5:InverterStatusWord received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is IInverterStatusWordFieldMessageData statusWordData)
                    {
                        var statusWord = new StatusWordBase(statusWordData.Value);
                        if (statusWord.IsFault
                            && this.machineSensorsStatus.IsMachineInNormalState
                            && (this.currentStateMachine == null || !this.currentStateMachine.GetType().ToString().Contains("PowerEnableStateMachine"))
                            )
                        {
                            this.logger.LogWarning($"6:Inverter fault detected in device {receivedMessage.DeviceIndex}! Set Power Enable Off.");
                            var powerEnableData = new PowerEnableMessageData(false);
                            this.CreatePowerEnableStateMachine(powerEnableData);
                        }

                        var msgData = new InverterStatusWordMessageData(receivedMessage.DeviceIndex, statusWordData.Value);
                        var msg = new NotificationMessage(
                            msgData,
                            "Inverter Status Word",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.InverterStatusWord,
                            MessageStatus.OperationExecuting);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                    }
                    break;

                case FieldMessageType.ShutterPositioning:
                    this.logger.LogTrace($"6:ShutterPositioning received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                    if (receivedMessage.Data is IInverterShutterPositioningFieldMessageData positioningData)
                    {
                        if (receivedMessage.Status == MessageStatus.OperationExecuting)
                        {
                            var msgData = new ShutterPositioningMessageData();
                            msgData.ShutterPosition = positioningData.ShutterPosition;
                            var msg = new NotificationMessage(
                                msgData,
                                "Inverter Shutter Positioning",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.ShutterPositioning,
                                MessageStatus.OperationExecuting);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                        }
                    }
                    break;

                // INFO Catch Exception from Inverter, to forward to the AS
                case FieldMessageType.InverterException:
                case FieldMessageType.InverterError:
                    {
                        var exceptionMessage = new InverterExceptionMessageData(null, receivedMessage.Description, 0);

                        var msg = new NotificationMessage(
                            exceptionMessage,
                            "Inverter Exception",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.InverterException,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                        break;
                    }
                // INFO Catch Exception from IoDriver, to forward to the AS
                case FieldMessageType.IoDriverException:
                    {
                        var ioExceptionMessage = new IoDriverExceptionMessageData(null, receivedMessage.Description, 0);

                        var msg = new NotificationMessage(
                            ioExceptionMessage,
                            "Io Driver Exception",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.IoDriverException,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<NotificationEvent>().Publish(msg);

                        break;
                    }
            }

            this.currentStateMachine?.ProcessFieldNotificationMessage(receivedMessage);
        }

        private void RetrieveIoDevicesConfigurationAsync()
        {
            this.isDataLayerReady = true;

            this.ioIndexDeviceList = this.vertimagConfiguration.GetInstalledIoList();

            this.machineSensorsStatus = new MachineSensorsStatus(this.machineConfigurationProvider.IsOneKMachine());
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
                    MessageStatus.OperationStart);

                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }
        }

        private void SendMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "FSM Error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.FsmException,
                MessageStatus.OperationError,
                ErrorLevel.Critical);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        private void SendStatusWordTimer(bool enable, int updateInterval)
        {
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, enable, updateInterval);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);
        }

        #endregion
    }
}
