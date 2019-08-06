using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    public class PositioningExecutingState : StateBase
    {
        #region Fields

        private readonly MachineSensorsStatus machineSensorsStatus;

        private readonly IPositioningMessageData positioningMessageData;

        private FieldCommandMessage commandMessage;

        private bool disposed;

        private decimal nTimes;

        private int numberExecutedSteps;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningMessageData positioningDownMessageData;

        private IPositioningFieldMessageData positioningFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        private IPositioningMessageData positioningUpMessageData;

        #endregion

        #region Constructors

        public PositioningExecutingState(
            IStateMachine parentMachine,
            IPositioningMessageData positioningMessageData,
            MachineSensorsStatus machineSensorsStatus,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.positioningMessageData = positioningMessageData;
            this.machineSensorsStatus = machineSensorsStatus;
        }

        #endregion

        #region Destructors

        ~PositioningExecutingState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.numberExecutedSteps++;
                        if (this.positioningMessageData.NumberCycles == 0 || this.numberExecutedSteps >= this.positioningMessageData.NumberCycles * 2)
                        {
                            this.Logger.LogDebug("FSM Finished Executing State");
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData, this.machineSensorsStatus, this.Logger, this.numberExecutedSteps));
                        }
                        else
                        {
                            // INFO Even to go Up and Odd for Down
                            this.commandMessage = new FieldCommandMessage(
                                this.numberExecutedSteps % 2 == 0 ? this.positioningUpFieldMessageData : this.positioningDownFieldMessageData,
                                $"Belt Burninshing moving cycle N° {this.numberExecutedSteps / 2}",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.Positioning);

                            this.Logger.LogTrace($"2:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);

                            var beltBurnishingPosition = this.numberExecutedSteps % 2 == 0 ? BeltBurnishingPosition.LowerBound : BeltBurnishingPosition.UpperBound;

                            var executedSteps = this.numberExecutedSteps / 2;

                            this.positioningMessageData.BeltBurnishingPosition = beltBurnishingPosition;
                            this.positioningMessageData.ExecutedCycles = executedSteps;

                            // Notification message
                            var notificationMessage = new NotificationMessage(
                                this.positioningMessageData,
                                $"Current position {beltBurnishingPosition}",
                                MessageActor.AutomationService,
                                MessageActor.FiniteStateMachines,
                                MessageType.CurrentPosition,
                                MessageStatus.OperationExecuting);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.Logger));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.InverterStatusUpdate)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationExecuting:
                        if (message.Data is InverterStatusUpdateFieldMessageData data)
                        {
                            if (this.IsNotificationToSend())
                            {
                                this.positioningMessageData.CurrentPosition = data.CurrentPosition;

                                var notificationMessage = new NotificationMessage(
                                    this.positioningMessageData,
                                    $"Current Encoder position: {data.CurrentPosition}",
                                    MessageActor.AutomationService,
                                    MessageActor.FiniteStateMachines,
                                    MessageType.Positioning,
                                    MessageStatus.OperationExecuting);

                                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            }
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Started Positioning Executing state with {this.positioningMessageData.NumberCycles} cycles");
            // INFO Hypothesis: The positioning has NumberCycles == 0
            if (this.positioningMessageData.NumberCycles == 0)
            {
                this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

                this.commandMessage = new FieldCommandMessage(
                    this.positioningFieldMessageData,
                    $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }
            else // INFO Hypothesis: Belt Burninshing Even for Up, Odd for Down
            {
                if (this.positioningMessageData.MovementType == MovementType.Relative)
                {
                    var distance = this.positioningMessageData.UpperBound - this.positioningMessageData.LowerBound;

                    // Build message for UP
                    this.positioningUpMessageData = new PositioningMessageData(
                        this.positioningMessageData.AxisMovement,
                        this.positioningMessageData.MovementType,
                        distance,
                        this.positioningMessageData.TargetSpeed,
                        this.positioningMessageData.TargetAcceleration,
                        this.positioningMessageData.TargetDeceleration,
                        this.positioningMessageData.NumberCycles,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.UpperBound);

                    // Build message for DOWN
                    this.positioningDownMessageData = new PositioningMessageData(
                        this.positioningMessageData.AxisMovement,
                        this.positioningMessageData.MovementType,
                        -distance,
                        this.positioningMessageData.TargetSpeed,
                        this.positioningMessageData.TargetAcceleration,
                        this.positioningMessageData.TargetDeceleration,
                        this.positioningMessageData.NumberCycles,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.UpperBound);
                }
                else
                {
                    // Build message for UP
                    this.positioningUpMessageData = new PositioningMessageData(
                        this.positioningMessageData.AxisMovement,
                        this.positioningMessageData.MovementType,
                        this.positioningMessageData.UpperBound,
                        this.positioningMessageData.TargetSpeed,
                        this.positioningMessageData.TargetAcceleration,
                        this.positioningMessageData.TargetDeceleration,
                        this.positioningMessageData.NumberCycles,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.UpperBound);

                    // Build message for DOWN
                    this.positioningDownMessageData = new PositioningMessageData(
                        this.positioningMessageData.AxisMovement,
                        this.positioningMessageData.MovementType,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.TargetSpeed,
                        this.positioningMessageData.TargetAcceleration,
                        this.positioningMessageData.TargetDeceleration,
                        this.positioningMessageData.NumberCycles,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.UpperBound);
                }

                this.positioningUpFieldMessageData = new PositioningFieldMessageData(this.positioningUpMessageData);

                this.positioningDownFieldMessageData = new PositioningFieldMessageData(this.positioningDownMessageData);

                // TEMP Hypothesis: in the case of Belt Burninshing the first TargetPosition is the upper bound
                this.commandMessage = new FieldCommandMessage(
                    this.positioningUpFieldMessageData,
                    "Belt Burninshing Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }

            this.Logger.LogTrace($"1:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData, this.machineSensorsStatus, this.Logger, this.numberExecutedSteps, true));
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

        private bool IsNotificationToSend()
        {
            const int NTIMES_TO_WAIT = 5;
            if (this.nTimes == NTIMES_TO_WAIT)
            {
                this.nTimes = 0;
                return true;
            }
            else
            {
                this.nTimes++;
                return false;
            }
        }

        #endregion
    }
}
