using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public abstract class FiniteStateMachine<TStartState, TErrorState> : IFiniteStateMachine, IDisposable
        where TStartState : IState
        where TErrorState : IState
    {
        #region Fields

        private readonly CommandEvent commandEvent;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue =
            new BlockingConcurrentQueue<CommandMessage>();

        private readonly Thread commandsDequeuingThread;

        private readonly NotificationEvent notificationEvent;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue =
            new BlockingConcurrentQueue<NotificationMessage>();

        private readonly Thread notificationsDequeuingThread;

        private readonly object syncObject = new object();

        private IState activeState;

        private SubscriptionToken commandEventSubscriptionToken;

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool isDisposed;

        private bool isStarted;

        private SubscriptionToken notificationEventSubscriptionToken;

        private BayNumber requestingBay;

        private IServiceProvider serviceProvider;

        #endregion

        #region Constructors

        protected FiniteStateMachine(
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.InstanceId = Guid.NewGuid();

            this.commandEvent = eventAggregator.GetEvent<CommandEvent>();
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();

            this.commandsDequeuingThread = new Thread(new ParameterizedThreadStart(this.DequeueCommands))
            {
                Name = $"[commands] {this.GetType().Name}",
            };

            this.notificationsDequeuingThread = new Thread(new ParameterizedThreadStart(this.DequeueNotifications))
            {
                Name = $"[notifications] {this.GetType().Name}",
            };
        }

        #endregion

        #region Events

        public event EventHandler<FiniteStateMachinesEventArgs> Completed;

        #endregion

        #region Properties

        public IState ActiveState
        {
            get => this.activeState;
            private set
            {
                lock (this.syncObject)
                {
                    if (this.activeState != value)
                    {
                        this.Logger.LogTrace($"Active state: {this.activeState?.GetType().Name}, new state {value?.GetType().Name} ");
                        this.activeState?.Exit();

                        if (this.activeState is IProgressMessageState progressMessageState)
                        {
                            if (progressMessageState.Message != null)
                            {
                                this.notificationEvent.Publish(progressMessageState.Message);
                            }
                        }

                        if (this.activeState is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }

                        this.activeState = value;
                        this.Logger.LogTrace($"Active state changed: {this.activeState?.GetType().Name} ");

                        try
                        {
                            this.activeState?.Enter(this.StartData, this.serviceProvider, this.MachineData);
                        }
                        catch (StateMachineException ex)
                        {
                            this.Logger.LogError(ex.NotificationMessage.Description, "Error while activating a State.");
                            this.notificationEvent.Publish(ex.NotificationMessage);

                            if (this.activeState == null)
                            {
                                var eventArgs = new FiniteStateMachinesEventArgs
                                {
                                    InstanceId = this.MachineData?.FsmId ?? this.InstanceId,
                                    NotificationMessage = ex.NotificationMessage,
                                };
                                this.RaiseCompleted(eventArgs);
                            }
                            else
                            {
                                this.activeState = this.activeState?.Stop(StopRequestReason.Error);
                                this.activeState?.Enter(this.StartData, this.serviceProvider, this.MachineData);
                            }
                        }

                        if (this.activeState is IStartMessageState startMessageState)
                        {
                            this.notificationEvent.Publish(startMessageState.Message);
                        }
                    }

                    // These code lines MUST be the last in the Setter
                    if (this.activeState is IEndState endState && endState.IsCompleted)
                    {
                        var eventArgs = new FiniteStateMachinesEventArgs
                        {
                            InstanceId = this.MachineData?.FsmId ?? this.InstanceId,
                            NotificationMessage = endState.EndMessage,
                        };

                        this.RaiseCompleted(eventArgs);
                    }
                }
            }
        }

        public Guid InstanceId { get; }

        public IFiniteStateMachineData MachineData { get; set; }

        public CommandMessage StartData { get; private set; }

        protected ILogger<StateBase> Logger { get; }

        #endregion

        #region Methods

        public void Abort()
        {
            this.ActiveState = this.OnAbort();
        }

        public virtual bool AllowMultipleInstances(CommandMessage command)
        {
            return true;
        }

        /// <summary>
        /// This code added is to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Pause()
        {
            this.ActiveState = this.OnPause();
        }

        public void Resume(CommandMessage commandMessage, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            lock (this.syncObject)
            {
                if (this.ActiveState is null)
                {
                    this.isStarted = true;

                    this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

                    this.StartData = commandMessage;

                    this.requestingBay = commandMessage.RequestingBay;

                    this.commandsDequeuingThread.Start(cancellationToken);
                    this.notificationsDequeuingThread.Start(cancellationToken);

                    this.InitializeSubscriptions();

                    this.ActiveState = this.GetState<TErrorState>();

                    return;
                }
                this.ActiveState = this.OnResume(commandMessage);
            }
        }

        public virtual void Start(CommandMessage commandMessage, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            if (commandMessage is null)
            {
                throw new ArgumentNullException(nameof(commandMessage));
            }

            lock (this.syncObject)
            {
                if (this.isStarted)
                {
                    throw new InvalidOperationException($"The state machine {this.GetType().Name} was already started");
                }

                if (this.OnStart(commandMessage, cancellationToken))
                {
                    this.isStarted = true;

                    this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

                    this.StartData = commandMessage;

                    this.requestingBay = commandMessage.RequestingBay;

                    this.commandsDequeuingThread.Start(cancellationToken);
                    this.notificationsDequeuingThread.Start(cancellationToken);

                    this.InitializeSubscriptions();

                    this.ActiveState = this.GetState<TStartState>();
                }
                else
                {
                    var eventArgs = new FiniteStateMachinesEventArgs
                    {
                        InstanceId = this.MachineData?.FsmId ?? this.InstanceId,
                    };
                    this.RaiseCompleted(eventArgs);
                }
            }
        }

        public void Stop(StopRequestReason reason)
        {
            lock (this.syncObject)
            {
                if (this.activeState is null)
                {
                    var eventArgs = new FiniteStateMachinesEventArgs
                    {
                        InstanceId = this.MachineData?.FsmId ?? this.InstanceId,
                    };
                    this.RaiseCompleted(eventArgs);
                }
                else
                {
                    this.ActiveState = this.OnStop(reason);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.commandEventSubscriptionToken?.Dispose();
            this.commandEventSubscriptionToken = null;

            this.notificationEventSubscriptionToken?.Dispose();
            this.notificationEventSubscriptionToken = null;

            this.commandQueue?.Dispose();
            this.notificationQueue?.Dispose();

            this.isDisposed = true;
        }

        protected abstract bool FilterCommand(CommandMessage command);

        protected abstract bool FilterNotification(NotificationMessage notification);

        protected IState GetState<TState>()
            where TState : IState
        {
            return this.serviceProvider.GetRequiredService<TState>();
        }

        protected virtual IState OnAbort()
        {
            return this.ActiveState.Abort();
        }

        protected virtual IState OnCommandReceived(CommandMessage command)
        {
            this.Logger.LogTrace($"{this.GetType().Name}: received command {command?.Type}, {command?.Description}");

            return this.ActiveState;
        }

        protected virtual IState OnNotificationReceived(NotificationMessage notification)
        {
            this.Logger.LogTrace($"{this.GetType().Name}: received notification {notification?.Type}, {notification?.Description}");

            return this.ActiveState;
        }

        protected virtual IState OnPause()
        {
            return this.ActiveState.Pause();
        }

        protected virtual IState OnResume(CommandMessage commandMessage)
        {
            return this.ActiveState.Resume(commandMessage);
        }

        protected virtual bool OnStart(CommandMessage commandMessage, CancellationToken cancellationToken)
        {
            return true;
        }

        protected virtual IState OnStop(StopRequestReason reason)
        {
            return this.ActiveState.Stop(reason);
        }

        protected void RaiseCompleted(FiniteStateMachinesEventArgs eventArgs)
        {
            this.Completed?.Invoke(this, eventArgs);
        }

        private void DequeueCommands(object cancellationTokenObject)
        {
            var cancellationToken = (CancellationToken)cancellationTokenObject;

            do
            {
                try
                {
                    if (this.commandQueue.TryDequeue(Timeout.Infinite, cancellationToken, out var commandMessage)
                        &&
                        commandMessage != null)
                    {
                        lock (this.syncObject)
                        {
                            this.ActiveState = this.OnCommandReceived(commandMessage);
                        }
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogTrace($"Terminating commands thread for service {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while processing a command.");
                    this.NotifyError(ex);

                    lock (this.syncObject)
                    {
                        if (this.activeState is null)
                        {
                            var eventArgs = new FiniteStateMachinesEventArgs
                            {
                                InstanceId = this.MachineData?.FsmId ?? this.InstanceId,
                            };
                            this.RaiseCompleted(eventArgs);
                        }
                        else
                        {
                            this.ActiveState = this.activeState?.Stop(StopRequestReason.Error);
                        }
                    }
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private void DequeueNotifications(object cancellationTokenObject)
        {
            var cancellationToken = (CancellationToken)cancellationTokenObject;

            do
            {
                try
                {
                    if (this.notificationQueue.TryDequeue(Timeout.Infinite, cancellationToken, out var notificationMessage)
                        &&
                        notificationMessage != null)
                    {
                        lock (this.syncObject)
                        {
                            this.ActiveState = this.OnNotificationReceived(notificationMessage);
                        }
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogTrace($"Terminating notifications thread for service {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while processing a notification.");
                    this.NotifyError(ex);

                    lock (this.syncObject)
                    {
                        if (this.activeState is null)
                        {
                            var eventArgs = new FiniteStateMachinesEventArgs
                            {
                                InstanceId = this.MachineData?.FsmId ?? this.InstanceId,
                            };
                            this.RaiseCompleted(eventArgs);
                        }
                        else
                        {
                            this.ActiveState = this.activeState?.Stop(StopRequestReason.Error);
                        }
                    }
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private void InitializeSubscriptions()
        {
            this.commandEventSubscriptionToken = this.commandEvent
                .Subscribe(
                    command => this.commandQueue.Enqueue(command),
                    ThreadOption.PublisherThread,
                    false,
                    this.FilterCommand);

            this.notificationEventSubscriptionToken = this.notificationEvent
                .Subscribe(
                    notification => this.notificationQueue.Enqueue(notification),
                    ThreadOption.PublisherThread,
                    false,
                    this.FilterNotification);
        }

        private void NotifyError(Exception ex)
        {
            this.notificationEvent.Publish(
                new NotificationMessage(
                    new FsmExceptionMessageData(ex, string.Empty, 0),
                    "FSM Error",
                    MessageActor.Any,
                    MessageActor.MachineManager,
                    MessageType.FsmException,
                    this.requestingBay,
                    BayNumber.None,
                    MessageStatus.OperationError,
                    ErrorLevel.Error));
        }

        #endregion
    }
}
