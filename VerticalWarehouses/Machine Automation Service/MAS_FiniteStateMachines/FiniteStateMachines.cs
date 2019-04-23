using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.BeltBreakIn;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> fieldNotificationQueue;

        private readonly Task fieldNotificationReceiveTask;

        private readonly ILogger<FiniteStateMachines> logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private IStateMachine currentStateMachine;

        private IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagment;

        private bool disposed;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public FiniteStateMachines(IEventAggregator eventAggregator, ILogger<FiniteStateMachines> logger, IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagment)
        {
            logger.LogDebug("1:Method Start");

            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.dataLayerConfigurationValueManagment = dataLayerConfigurationValueManagment;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.fieldNotificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task(this.CommandReceiveTaskFunction);
            this.notificationReceiveTask = new Task(this.NotificationReceiveTaskFunction);
            this.fieldNotificationReceiveTask = new Task(this.FieldNotificationReceiveTaskFunction);

            this.logger.LogTrace("2:Subscription Command");

            this.InitializeMethodSubscriptions();

            logger.LogDebug("3:Method End");
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
            this.logger.LogDebug("1:Method Start");

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

                throw new FiniteStateMachinesException($"Exception: {ex.Message} while starting service threads", FiniteStateMachinesExceptionCode.ServiceTaskStartFailure, ex);
            }

            this.logger.LogDebug("3:Method End");

            await Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                if (this.currentStateMachine != null && receivedMessage.Type != MessageType.Stop)
                {
                    var errorNotification = new NotificationMessage(null, "Inverter operation already in progress", MessageActor.Any,
                        MessageActor.FiniteStateMachines, receivedMessage.Type, MessageStatus.OperationError, ErrorLevel.Error);

                    this.logger.LogTrace($"4:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

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

                    case MessageType.Positioning:
                        this.ProcessPositioningMessage(receivedMessage);
                        break;

                    case MessageType.ShutterPositioning:
                        this.ProcessShutterPositioningMessage(receivedMessage);
                        break;

                    case MessageType.BeltBreakIn:
                        this.ProcessBeltBreakInMessage(receivedMessage);
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("5:Method End");
        }

        private void FieldNotificationReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                FieldNotificationMessage receivedMessage;
                try
                {
                    this.fieldNotificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Field Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.CalibrateAxis:
                    case FieldMessageType.InverterReset:
                        break;

                    case FieldMessageType.SensorsChanged:
                        this.logger.LogTrace($"4:IOSensorsChanged received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                        if (receivedMessage.Data is ISensorsChangedFieldMessageData data)
                        {
                            var msgData = new SensorsChangedMessageData();
                            msgData.SensorsStates = data.SensorsStates;

                            var msg = new NotificationMessage(
                                msgData,
                                "IO sensors status",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.SensorsChanged,
                                MessageStatus.OperationExecuting,
                                ErrorLevel.NoError);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                        }
                        break;

                    case FieldMessageType.InverterStatusUpdate:
                        this.logger.LogTrace($"5:InverterStatusUpdate received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                        break;
                }
                this.currentStateMachine?.ProcessFieldNotificationMessage(receivedMessage);
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("6:Method End");
        }

        private void InitializeMethodSubscriptions()
        {
            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(message =>
                {
                    this.commandQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(message =>
                {
                    this.notificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            var fieldNotificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            fieldNotificationEvent.Subscribe(message =>
                {
                    this.fieldNotificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.FiniteStateMachines || message.Destination == FieldMessageActor.Any);
        }

        private void NotificationReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.DataLayerReady:
                        var fieldNotification = new FieldNotificationMessage(null,
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
                                    // update the installation status homing flag in the dataLayer
                                    this.dataLayerConfigurationValueManagment.SetBoolConfigurationValueAsync(
                                        (long)SetupStatus.VerticalHomingDone,
                                        (long)ConfigurationCategory.SetupStatus,
                                        true);

                                    this.logger.LogTrace($"4:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                case MessageStatus.OperationStop:

                                    this.logger.LogTrace($"4:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                    this.currentStateMachine = null;

                                    break;

                                default:
                                    break;
                            }
                        }
                        break;

                    case MessageType.Positioning:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            if (receivedMessage.Status == MessageStatus.OperationEnd ||
                                receivedMessage.Status == MessageStatus.OperationStop)
                            {
                                this.logger.LogTrace($"5:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                this.currentStateMachine = null;
                            }
                        }
                        break;

                    case MessageType.ShutterPositioning:
                        if (receivedMessage.Source == MessageActor.FiniteStateMachines)
                        {
                            if (receivedMessage.Status == MessageStatus.OperationEnd ||
                                receivedMessage.Status == MessageStatus.OperationStop)
                            {
                                this.logger.LogTrace($"6:Deallocation FSM {this.currentStateMachine?.GetType()}");
                                this.currentStateMachine = null;
                            }
                        }
                        break;
                }

                this.currentStateMachine?.ProcessNotificationMessage(receivedMessage);
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("6:Method End");
        }

        private void ProcessBeltBreakInMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.Data is IPositioningMessageData data)
            {
                this.currentStateMachine = new BeltBreakInStateMachine(this.eventAggregator, data, this.logger);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");
                this.currentStateMachine.Start();
            }

            this.logger.LogDebug("3:Method End");
        }

        private void ProcessHomingMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.Data is IHomingMessageData data)
            {
                this.currentStateMachine = new HomingStateMachine(this.eventAggregator, data, this.logger);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");
                this.currentStateMachine.Start();
            }

            this.logger.LogDebug("3:Method End");
        }

        private void ProcessPositioningMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.Data is IPositioningMessageData data)
            {
                this.currentStateMachine = new PositioningStateMachine(this.eventAggregator, data, this.logger);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");
                this.currentStateMachine.Start();
            }

            this.logger.LogDebug("3:Method End");
        }

        private void ProcessShutterPositioningMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.Data is IShutterPositioningMessageData data)
            {
                this.currentStateMachine = new ShutterPositioningStateMachine(this.eventAggregator, data, this.logger);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");
                this.currentStateMachine.Start();
            }

            this.logger.LogDebug("3:Method End");
        }

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");
            this.currentStateMachine.ProcessCommandMessage(receivedMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
