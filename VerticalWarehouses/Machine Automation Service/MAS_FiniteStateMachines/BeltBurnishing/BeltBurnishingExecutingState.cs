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

namespace Ferretto.VW.MAS_FiniteStateMachines.BeltBurnishing
{
    public class BeltBurnishingExecutingState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IPositioningFieldMessageData positioningDownFieldMessageData;

        private readonly IPositioningMessageData positioningDownMessageData;

        private readonly IPositioningFieldMessageData positioningFieldMessageData;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly IPositioningFieldMessageData positioningUpFieldMessageData;

        private readonly IPositioningMessageData positioningUpMessageData;

        private FieldCommandMessage commandMessage;

        private int numberExecutedSteps;

        #endregion

        #region Constructors

        public BeltBurnishingExecutingState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentMachine;

            this.positioningMessageData = positioningMessageData;

            // INFO Hypothesis: The positioning has numberCycles = 0
            if (this.positioningMessageData.NumberCycles == 0)
            {
                this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

                this.commandMessage = new FieldCommandMessage(this.positioningFieldMessageData,
                    $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }
            else // INFO Hypothesis: Belt Break-In Even for Up, Odd for Down
            {
                this.positioningUpMessageData = new PositioningMessageData(this.positioningMessageData.AxisMovement,
                                                                      this.positioningMessageData.MovementType,
                                                                      this.positioningMessageData.UpperBound,
                                                                      this.positioningMessageData.TargetSpeed,
                                                                      this.positioningMessageData.TargetAcceleration,
                                                                      this.positioningMessageData.TargetDeceleration,
                                                                      this.positioningMessageData.NumberCycles,
                                                                      this.positioningMessageData.LowerBound,
                                                                      this.positioningMessageData.UpperBound);

                this.positioningDownMessageData = new PositioningMessageData(this.positioningMessageData.AxisMovement,
                                                                      this.positioningMessageData.MovementType,
                                                                      this.positioningMessageData.LowerBound,
                                                                      this.positioningMessageData.TargetSpeed,
                                                                      this.positioningMessageData.TargetAcceleration,
                                                                      this.positioningMessageData.TargetDeceleration,
                                                                      this.positioningMessageData.NumberCycles,
                                                                      this.positioningMessageData.LowerBound,
                                                                      this.positioningMessageData.UpperBound);

                this.positioningUpFieldMessageData = new PositioningFieldMessageData(this.positioningUpMessageData);

                this.positioningDownFieldMessageData = new PositioningFieldMessageData(this.positioningDownMessageData);

                // TEMP Hypothesis: in the case of belt break-in the first TargetPosition is the upper bound
                this.commandMessage = new FieldCommandMessage(this.positioningUpFieldMessageData,
                    "Belt Break-In Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }

            this.logger.LogTrace($"2:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:

                        this.commandMessage = new FieldCommandMessage(null,
                            $"{this.positioningMessageData.AxisMovement} Movement Start Request",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.Positioning);

                        this.logger.LogTrace($"3:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

                        break;

                    case MessageStatus.OperationEnd:
                        this.numberExecutedSteps++;

                        if (this.positioningMessageData.NumberCycles == 0 || this.numberExecutedSteps >= this.positioningMessageData.NumberCycles * 2)
                        {
                            this.ParentStateMachine.ChangeState(new BeltBurnishingEndState(this.ParentStateMachine, this.positioningMessageData, this.logger, this.numberExecutedSteps));
                        }
                        else
                        {
                            // INFO Even for Up, Odd for Down
                            this.commandMessage = new FieldCommandMessage(this.numberExecutedSteps % 2 == 0 ? this.positioningUpFieldMessageData : this.positioningDownFieldMessageData,
                                $"Belt Break-In moving cycle N° {this.numberExecutedSteps / 2}",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.Positioning);

                            this.logger.LogTrace($"4:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);

                            var beltBreakInPosition = this.numberExecutedSteps % 2 == 0 ? BeltBurnishingPosition.LowerBound : BeltBurnishingPosition.UpperBound;

                            var executedSteps = this.numberExecutedSteps / 2;

                            var currentPositionMessageData = new CurrentPositionMessageData(executedSteps, beltBreakInPosition);

                            // Notification message
                            var notificationMessage = new NotificationMessage(currentPositionMessageData,
                                $"Current position {beltBreakInPosition}",
                                MessageActor.AutomationService,
                                MessageActor.FiniteStateMachines,
                                MessageType.CurrentEncoderPosition,
                                MessageStatus.OperationExecuting
                            );

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new BeltBurnishingErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
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
                            var notificationMessage = new NotificationMessage(new CurrentPositionMessageData(data.CurrentPosition),
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
                        this.ParentStateMachine.ChangeState(new BeltBurnishingErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                        break;
                }
            }
            this.logger.LogDebug("5:Method End");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.logger.LogDebug("3:Method End");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new BeltBurnishingEndState(this.ParentStateMachine, this.positioningMessageData, this.logger, this.numberExecutedSteps, true));

            this.logger.LogDebug("2:Method End");
        }

        #endregion
    }
}
