using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    internal class PositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IPositioningMessageData positioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStateMachine(
            IMachineSensorsStatus machineSensorsStatus,
            IEventAggregator eventAggregator,
            IPositioningMessageData positioningMessageData,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.logger = logger;

            this.logger.LogTrace("1:Method Start");

            this.logger.LogTrace($"TargetPosition = {positioningMessageData.TargetPosition} - CurrentPosition = {positioningMessageData.CurrentPosition} - MovementType = {positioningMessageData.MovementType}");

            this.CurrentState = new EmptyState(logger);

            this.positioningMessageData = positioningMessageData;

            this.machineSensorsStatus = machineSensorsStatus;
        }

        #endregion

        #region Destructors

        ~PositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        public override void Start()
        {
            bool checkConditions;

            //INFO Begin check the pre conditions to start the positioning
            lock (this.CurrentState)
            {
                //INFO Check the Horizontal and Vertical conditions for Positioning
                checkConditions = this.CheckConditions();

                if (checkConditions)
                {
                    if (this.positioningMessageData.MovementMode == MovementMode.FindZero && this.machineSensorsStatus.IsSensorZeroOnCradle)
                    {
                        this.CurrentState = new PositioningEndState(this, this.machineSensorsStatus, this.positioningMessageData, this.logger, 0);
                    }
                    else
                    {
                        this.CurrentState = new PositioningStartState(this, this.machineSensorsStatus, this.positioningMessageData, this.logger);
                    }
                }
                else
                {
                    var notificationMessage = new NotificationMessage(
                        this.positioningMessageData,
                        "Conditions not verified for positioning",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.InverterException,
                        MessageStatus.OperationStart);

                    this.Logger.LogError($"Conditions not verified for positioning");

                    this.PublishNotificationMessage(notificationMessage);
                    this.CurrentState = new PositioningErrorState(this, this.machineSensorsStatus, this.positioningMessageData, null, this.Logger);
                }

                this.CurrentState?.Start();
            }
            //INFO End check the pre conditions to start the positioning

            this.logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

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
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        private bool CheckConditions()
        {
            //HACK The condition must be handled by the Bug #3711
            //INFO For the Belt Burnishing the positioning is allowed only without a drawer.

            return (((this.machineSensorsStatus.IsDrawerCompletelyOnCradle && !this.machineSensorsStatus.IsSensorZeroOnCradle /*&& this.positioningMessageData.MovementMode == MovementMode.Position*/) ||
                this.machineSensorsStatus.IsDrawerCompletelyOffCradle && this.machineSensorsStatus.IsSensorZeroOnCradle
                ) &&
                this.positioningMessageData.AxisMovement == Axis.Vertical)
                ||
                this.positioningMessageData.AxisMovement == Axis.Horizontal;
        }

        #endregion
    }
}
