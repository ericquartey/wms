using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningExecutingState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private FieldCommandMessage commandMessage;

        private bool disposed;

        private int numberExecutedSteps;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningFieldMessageData positioningFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        private IPositioningMessageData verticalPositioningDownMessageData;

        private IPositioningMessageData verticalPositioningMessageData;

        private IPositioningMessageData verticalPositioningUpMessageData;

        #endregion

        #region Constructors

        public PositioningExecutingState(IStateMachine parentMachine, IPositioningMessageData verticalPositioningMessageData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.verticalPositioningMessageData = verticalPositioningMessageData;
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
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.numberExecutedSteps++;

                        if (this.verticalPositioningMessageData.NumberCycles == 0 || this.numberExecutedSteps > this.verticalPositioningMessageData.NumberCycles * 2)
                        {
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.verticalPositioningMessageData, this.logger, this.numberExecutedSteps));
                        }
                        else
                        {
                            this.logger.LogTrace($"3A:Condition {this.numberExecutedSteps % 2 == 0} - Step eseguiti {this.numberExecutedSteps}");

                            // INFO Even to go Up and Odd for Down
                            this.commandMessage = new FieldCommandMessage(this.numberExecutedSteps % 2 == 0 ? this.positioningUpFieldMessageData : this.positioningDownFieldMessageData,
                                $"Belt Burninshing moving cycle N° {this.numberExecutedSteps / 2}",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.Positioning);

                            this.logger.LogTrace($"4:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);

                            // INFO To indicate the next position
                            var beltBurnishingPosition = this.numberExecutedSteps % 2 == 0 ? BeltBurnishingPosition.LowerBound : BeltBurnishingPosition.UpperBound;

                            var executedSteps = this.numberExecutedSteps / 2;

                            this.verticalPositioningMessageData.BeltBurnishingPosition = beltBurnishingPosition;
                            this.verticalPositioningMessageData.ExecutedCycles = executedSteps;

                            // Notification message
                            var notificationMessage = new NotificationMessage(this.verticalPositioningMessageData,
                                $"Current position {beltBurnishingPosition}",
                                MessageActor.AutomationService,
                                MessageActor.FiniteStateMachines,
                                MessageType.CurrentEncoderPosition,
                                MessageStatus.OperationExecuting
                            );

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.verticalPositioningMessageData, message, this.logger));
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
                            this.verticalPositioningMessageData.CurrentPosition = data.CurrentPosition;

                            var notificationMessage = new NotificationMessage(this.verticalPositioningMessageData,
                                $"Current Encoder position: {data.CurrentPosition}",
                                MessageActor.AutomationService,
                                MessageActor.FiniteStateMachines,
                                MessageType.CurrentEncoderPosition,
                                MessageStatus.OperationExecuting
                            );

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.verticalPositioningMessageData, message, this.logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            // INFO Hypothesis: The positioning has NumberCycles == 0
            if (this.verticalPositioningMessageData.NumberCycles == 0)
            {
                this.positioningFieldMessageData = new PositioningFieldMessageData(this.verticalPositioningMessageData);

                this.commandMessage = new FieldCommandMessage(this.positioningFieldMessageData,
                    $"{this.verticalPositioningMessageData.AxisMovement} Positioning State Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }
            else // INFO Hypothesis: Belt Burninshing Even for Up, Odd for Down
            {
                this.verticalPositioningUpMessageData = new PositioningMessageData(this.verticalPositioningMessageData.AxisMovement,
                                                                      this.verticalPositioningMessageData.MovementType,
                                                                      this.verticalPositioningMessageData.UpperBound,
                                                                      this.verticalPositioningMessageData.TargetSpeed,
                                                                      this.verticalPositioningMessageData.TargetAcceleration,
                                                                      this.verticalPositioningMessageData.TargetDeceleration,
                                                                      this.verticalPositioningMessageData.NumberCycles,
                                                                      this.verticalPositioningMessageData.LowerBound,
                                                                      this.verticalPositioningMessageData.UpperBound,
                                                                      this.verticalPositioningMessageData.Resolution);

                this.verticalPositioningDownMessageData = new PositioningMessageData(this.verticalPositioningMessageData.AxisMovement,
                                                                      this.verticalPositioningMessageData.MovementType,
                                                                      this.verticalPositioningMessageData.LowerBound,
                                                                      this.verticalPositioningMessageData.TargetSpeed,
                                                                      this.verticalPositioningMessageData.TargetAcceleration,
                                                                      this.verticalPositioningMessageData.TargetDeceleration,
                                                                      this.verticalPositioningMessageData.NumberCycles,
                                                                      this.verticalPositioningMessageData.LowerBound,
                                                                      this.verticalPositioningMessageData.UpperBound,
                                                                      this.verticalPositioningMessageData.Resolution);

                this.positioningUpFieldMessageData = new PositioningFieldMessageData(this.verticalPositioningUpMessageData);

                this.positioningDownFieldMessageData = new PositioningFieldMessageData(this.verticalPositioningDownMessageData);

                // TEMP Hypothesis: in the case of Belt Burninshing the first TargetPosition is the upper bound
                this.commandMessage = new FieldCommandMessage(this.positioningUpFieldMessageData,
                    "Belt Burninshing Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }

            this.logger.LogTrace($"2:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.verticalPositioningMessageData, this.logger, this.numberExecutedSteps, true));
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

        #endregion
    }
}
