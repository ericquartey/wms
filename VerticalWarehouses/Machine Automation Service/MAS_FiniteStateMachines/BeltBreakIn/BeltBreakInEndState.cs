using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.BeltBreakIn
{
    public class BeltBreakInEndState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly int numberExecutedSteps;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly bool stopRequested;

        private FieldCommandMessage stopMessage;

        private ResetInverterFieldMessageData stopMessageData;

        #endregion

        #region Constructors

        public BeltBreakInEndState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger, int numberExecutedSteps, bool stopRequested = false)
        {
            try
            {
                this.logger = logger;
                this.logger?.LogDebug("1:Method Start");

                this.stopRequested = stopRequested;
                this.ParentStateMachine = parentMachine;
                this.positioningMessageData = positioningMessageData;
                this.numberExecutedSteps = numberExecutedSteps;

                if (positioningMessageData.NumberCycles == 0)
                {
                    this.stopMessageData = new ResetInverterFieldMessageData(this.positioningMessageData.AxisMovement);
                    this.stopMessage = new FieldCommandMessage(this.stopMessageData,
                        $"Reset Inverter Axis {this.positioningMessageData.AxisMovement}",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterReset);
                }
                else
                {
                    this.stopMessageData = new ResetInverterFieldMessageData(this.positioningMessageData.NumberCycles);
                    this.stopMessage = new FieldCommandMessage(this.stopMessageData,
                        $"Reset Inverter at cycle {this.numberExecutedSteps / 2}",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterReset);
                }

                this.logger?.LogTrace($"2:Publish Field Command Message processed: {this.stopMessage.Type}, {this.stopMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(this.stopMessage);

                this.logger?.LogDebug("3:Method End");
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException();
            }
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

            this.logger.LogTrace($"2:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterReset:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:
                            var notificationMessage = new NotificationMessage(
                                null,
                                this.positioningMessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Break-In Completed",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.BeltBreakIn,
                                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new BeltBreakInErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                            break;
                    }
                    break;
            }

            this.logger.LogDebug("3:Method End");
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
        }

        #endregion
    }
}
