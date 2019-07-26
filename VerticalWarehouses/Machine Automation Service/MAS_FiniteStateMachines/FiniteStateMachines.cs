using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.FiniteStateMachines
{
    public partial class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue;

        private readonly Task fieldNotificationReceiveTask;

        private readonly ILogger<FiniteStateMachines> logger;

        private readonly MachineSensorsStatus machineSensorsStatus;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly IVertimagConfigurationDataLayer vertimagConfiguration;

        private IStateMachine currentStateMachine;

        private bool disposed;

        private bool forceInverterIoStatusPublish;

        private bool forceRemoteIoStatusPublish;

        private List<IoIndex> ioIndexDeviceList;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public FiniteStateMachines(
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IVertimagConfigurationDataLayer vertimagConfiguration)
        {
            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;

            this.vertimagConfiguration = vertimagConfiguration;

            this.machineSensorsStatus = new MachineSensorsStatus();

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

        #region Destructors

        ~FiniteStateMachines()
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
            }

            this.disposed = true;
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

                if (this.currentStateMachine != null && receivedMessage.Type != MessageType.Stop && receivedMessage.Type != MessageType.SensorsChanged)
                {
                    var errorNotification = new NotificationMessage(
                        null,
                        "Inverter operation already in progress",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        receivedMessage.Type,
                        MessageStatus.OperationError,
                        ErrorLevel.Error);

                    this.logger.LogTrace($"3:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

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

                    case MessageType.ShutterControl:
                        this.ProcessShutterControlMessage(receivedMessage);
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
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void FieldNotificationReceiveTaskFunction()
        {
            NotificationMessage msg;
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
                    this.logger.LogDebug($"2:Exception: {ex.Message}");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.CalibrateAxis:
                    case FieldMessageType.InverterPowerOff:
                        break;

                    case FieldMessageType.SensorsChanged:

                        this.logger.LogTrace($"3:IOSensorsChanged received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                        if (receivedMessage.Data is ISensorsChangedFieldMessageData dataIOs)
                        {
                            var ioIndex = receivedMessage.DeviceIndex;

                            if (this.machineSensorsStatus.UpdateInputs(ioIndex, dataIOs.SensorsStates, receivedMessage.Source) || this.forceRemoteIoStatusPublish)
                            {
                                var msgData = new SensorsChangedMessageData();

                                msgData.SensorsStates = this.machineSensorsStatus.DisplayedInputs;

                                msg = new NotificationMessage(
                                    msgData,
                                    "IO sensors status",
                                    MessageActor.Any,
                                    MessageActor.FiniteStateMachines,
                                    MessageType.SensorsChanged,
                                    MessageStatus.OperationExecuting);
                                this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                                this.forceRemoteIoStatusPublish = false;
                            }
                        }
                        break;

                    case FieldMessageType.InverterStatusUpdate:

                        this.logger.LogTrace($"4:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                        if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData dataInverters)
                        {
                            var inverterIndex = receivedMessage.DeviceIndex;

                            if (this.machineSensorsStatus.UpdateInputs(inverterIndex, dataInverters.CurrentSensorStatus, receivedMessage.Source) || this.forceInverterIoStatusPublish)
                            {
                                var msgData = new SensorsChangedMessageData();
                                msgData.SensorsStates = this.machineSensorsStatus.DisplayedInputs;

                                msg = new NotificationMessage(
                                    msgData,
                                    "IO sensors status",
                                    MessageActor.Any,
                                    MessageActor.FiniteStateMachines,
                                    MessageType.SensorsChanged,
                                    MessageStatus.OperationExecuting);
                                this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                                this.forceInverterIoStatusPublish = false;
                            }
                        }
                        break;

                    // INFO Catch Exception from Inverter, to forward to the AS
                    case FieldMessageType.InverterException:
                    case FieldMessageType.InverterError:
                        var exceptionMessage = new InverterExceptionMessageData(null, receivedMessage.Description, 0);

                        msg = new NotificationMessage(
                            exceptionMessage,
                            "Inverter Exception",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.InverterException,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                        break;

                    // INFO Catch Exception from IoDriver, to forward to the AS
                    case FieldMessageType.IoDriverException:
                        var ioExceptionMessage = new IoDriverExceptionMessageData(null, receivedMessage.Description, 0);

                        msg = new NotificationMessage(
                            ioExceptionMessage,
                            "Inverter Exception",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.IoDriverException,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<NotificationEvent>().Publish(msg);

                        break;
                }
                this.currentStateMachine?.ProcessFieldNotificationMessage(receivedMessage);
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
                    this.logger.LogTrace($"Enqueue Field Notification message: {message.Type}, Source: {message.Source}, Destination {message.Destination}, Status: {message.Status}");
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
                            MessageStatus.NoStatus);

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
                                        // update the installation status homing flag in the dataLayer
                                        this.dataLayerConfigurationValueManagement.SetBoolConfigurationValue(
                                            (long)SetupStatus.VerticalHomingDone,
                                            (long)ConfigurationCategory.SetupStatus,
                                            true);
                                    }
                                    catch (Exception ex)
                                    {
                                        this.logger.LogDebug($"4:Exception: {ex.Message}");

                                        this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                                    }

                                    this.logger.LogTrace($"5:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"6:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"7:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;

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

                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"9:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"10:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;

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

                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"12:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"13:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;

                                    //TODO: According to the type of error we can try to resolve here
                                    break;
                            }
                        }
                        break;

                    case MessageType.ShutterControl:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            switch (receivedMessage.Status)
                            {
                                case MessageStatus.OperationEnd:

                                    this.logger.LogTrace($"14:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"15:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationError:

                                    this.logger.LogTrace($"16:Deallocation FSM {this.currentStateMachine?.GetType()} for error");
                                    this.currentStateMachine = null;

                                    //TODO: According to the type of error we can try to resolve here
                                    break;
                            }
                        }
                        break;
                }

                this.currentStateMachine?.ProcessNotificationMessage(receivedMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void RetrieveIoDevicesConfigurationAsync()
        {
            this.ioIndexDeviceList = this.vertimagConfiguration.GetInstalledIoList();
        }

        private void SendMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "FSM Error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.FSMException,
                MessageStatus.OperationError,
                ErrorLevel.Critical);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        #endregion
    }
}
