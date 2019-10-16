using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public abstract class FiniteStateMachine<TStartState> : IFiniteStateMachine, IDisposable
        where TStartState : IState
    {
        #region Fields

        private readonly CommandEvent commandEvent;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue = new BlockingConcurrentQueue<CommandMessage>();

        private readonly NotificationEvent notificationEvent;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

        private readonly IServiceScope serviceScope;

        private IState activeState;

        private SubscriptionToken commandEventSubscriptionToken;

        private Thread commandsDequeuingThread;

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool isDisposed;

        private bool isStarted;

        private SubscriptionToken notificationEventSubscriptionToken;

        private Thread notificationsDequeuingThread;

        private BayNumber requestingBay;

        #endregion

        #region Constructors

        protected FiniteStateMachine(
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.InstanceId = Guid.NewGuid();

            this.commandEvent = eventAggregator.GetEvent<CommandEvent>();
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.serviceScope = serviceScopeFactory.CreateScope();
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
                if (this.activeState != value)
                {
                    this.activeState?.Exit();

                    if (this.activeState is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    this.activeState = value;

                    this.activeState?.Enter(this.StartData);
                }

                // These code lines MUST be the last in the Setter
                if (this.activeState is IEndState endState && endState.IsCompleted)
                {
                    var eventArgs = new FiniteStateMachinesEventArgs { InstanceId = this.InstanceId, NotificationMessage = endState.EndMessage };
                    this.RaiseCompleted(eventArgs);
                }
            }
        }

        public Guid InstanceId { get; }

        public CommandMessage StartData { get; private set; }

        protected ILogger<StateBase> Logger { get; }

        #endregion

        #region Methods

        /// <summary>
        /// This code added is to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.commandEventSubscriptionToken?.Dispose();
                this.commandEventSubscriptionToken = null;

                this.notificationEventSubscriptionToken?.Dispose();
                this.notificationEventSubscriptionToken = null;

                this.serviceScope.Dispose();

                this.OnDisposing();

                this.isDisposed = true;
            }
        }

        public void Start(CommandMessage commandMessage, CancellationToken cancellationToken)
        {
            if (this.isStarted)
            {
                throw new InvalidOperationException($"The state machine {this.GetType().Name} was already started");
            }

            this.isStarted = true;

            this.StartData = commandMessage;

            this.requestingBay = commandMessage.RequestingBay;

            this.commandsDequeuingThread = new Thread(this.DequeueCommands)
            {
                Name = $"MM Commands {this.GetType().Name}",
            };

            this.notificationsDequeuingThread = new Thread(this.DequeueNotifications)
            {
                Name = $"MM Notifications {this.GetType().Name}",
            };

            this.commandsDequeuingThread.Start(cancellationToken);
            this.notificationsDequeuingThread.Start(cancellationToken);

            this.InitializeSubscriptions();

            this.ActiveState = this.GetState<TStartState>();
        }

        public void Stop(StopRequestReason reason)
        {
            this.OnStop(reason);
        }

        protected abstract bool FilterCommand(CommandMessage command);

        protected abstract bool FilterNotification(NotificationMessage notification);

        protected IState GetState<TState>()
            where TState : IState
        {
            return this.serviceScope.ServiceProvider.GetRequiredService<TState>();
        }

        protected virtual IState OnCommandReceived(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {command.Type}, {command.Description}");

            return this.ActiveState;
        }

        protected virtual void OnDisposing()
        {
            // do nothing
            // derived classes can customize the behaviour of this method
        }

        protected virtual IState OnNotificationReceived(NotificationMessage notification)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received notification {notification.Type}, {notification.Description}");

            return this.ActiveState;
        }

        protected virtual void OnStart(CommandMessage commandMessage, CancellationToken cancellationToken)
        {
            // do nothing
        }

        protected virtual void OnStop(StopRequestReason reason)
        {
            this.ActiveState = this.ActiveState.Stop(reason);
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
                    this.commandQueue.TryDequeue(Timeout.Infinite, cancellationToken, out var commandMessage);

                    this.ActiveState = this.OnCommandReceived(commandMessage);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    this.NotifyError(ex);

                    return;
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
                    this.notificationQueue.TryDequeue(Timeout.Infinite, cancellationToken, out var notificationMessage);

                    this.ActiveState = this.OnNotificationReceived(notificationMessage);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    this.NotifyError(ex);

                    return;
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
            this.Logger.LogDebug($"Notifying error: {ex.Message}");

            var msg = new NotificationMessage(
                new FsmExceptionMessageData(ex, string.Empty, 0),
                "FSM Error",
                MessageActor.Any,
                MessageActor.MissionsManager,
                MessageType.FsmException,
                this.requestingBay,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.notificationEvent.Publish(msg);
        }

        #endregion
    }
}
