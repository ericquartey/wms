using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningStateMachine : StateMachineBase
    {

        #region Fields

        private readonly IShutterPositioningStateMachineData shutterPositioningStateMachineData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(
            IShutterPositioningStateMachineData shutterPositioningStateMachineData)
            : base(shutterPositioningStateMachineData.EventAggregator, shutterPositioningStateMachineData.Logger, shutterPositioningStateMachineData.ServiceScopeFactory)
        {
            this.CurrentState = new EmptyState(shutterPositioningStateMachineData.Logger);

            this.shutterPositioningStateMachineData = shutterPositioningStateMachineData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.shutterPositioningStateMachineData.DelayTimer?.Dispose();
            this.shutterPositioningStateMachineData.DelayTimer = new Timer(this.DelayTimerMethod, null, -1, Timeout.Infinite);

            lock (this.CurrentState)
            {
                if (!this.shutterPositioningStateMachineData.MachineSensorsStatus.IsMachineInRunningState ||
                    this.shutterPositioningStateMachineData.MachineSensorsStatus.IsDrawerPartiallyOnCradleBay1 ||
                    !(this.shutterPositioningStateMachineData.PositioningMessageData.MovementMode == MovementMode.Position || this.shutterPositioningStateMachineData.PositioningMessageData.MovementMode == MovementMode.TestLoop)
                    )
                {
                    this.CurrentState = new ShutterPositioningErrorState(this, this.shutterPositioningStateMachineData, null);
                }
                else
                {
                    this.CurrentState = new ShutterPositioningStartState(this, this.shutterPositioningStateMachineData);
                }

                this.CurrentState?.Start();
            }

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }
            if (disposing)
            {
                this.shutterPositioningStateMachineData.DelayTimer?.Dispose();
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        private void DelayTimerMethod(object state)
        {
            // stop timer
            this.shutterPositioningStateMachineData.DelayTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // send a notification to wake up the state machine waiting for the delay
            var notificationMessage = new NotificationMessage(
                null,
                "Delay Timer Expired",
                MessageActor.FiniteStateMachines,
                MessageActor.FiniteStateMachines,
                MessageType.CheckCondition,
                this.shutterPositioningStateMachineData.RequestingBay,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"1:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        #endregion
    }
}
