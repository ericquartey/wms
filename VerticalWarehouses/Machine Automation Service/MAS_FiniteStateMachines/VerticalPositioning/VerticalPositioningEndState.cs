using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalPositioning
{
    public class VerticalPositioningEndState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly int numberExecutedSteps;

        private readonly FieldCommandMessage stopMessage;

        private readonly bool stopRequested;

        private readonly IVerticalPositioningMessageData verticalPositioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public VerticalPositioningEndState(IStateMachine parentMachine, IVerticalPositioningMessageData verticalPositioningMessageData, ILogger logger,
            int numberExecutedSteps, bool stopRequested = false)
        {
            try
            {
                this.logger = logger;
                this.logger?.LogDebug("1:Method Start");

                this.stopRequested = stopRequested;
                this.ParentStateMachine = parentMachine;
                this.verticalPositioningMessageData = verticalPositioningMessageData;
                this.numberExecutedSteps = numberExecutedSteps;

                if (this.verticalPositioningMessageData.NumberCycles == 0)
                {
                    this.stopMessage = new FieldCommandMessage(null,
                        $"Reset Inverter Axis {this.verticalPositioningMessageData.AxisMovement}",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop);
                }
                else
                {
                    this.stopMessage = new FieldCommandMessage(null,
                        $"Reset Inverter at cycle {this.numberExecutedSteps / 2}",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop);
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

        #region Destructors

        ~VerticalPositioningEndState()
        {
            this.Dispose(false);
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
                case FieldMessageType.InverterStop:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:
                            var notificationMessage = new NotificationMessage(
                                null,
                                this.verticalPositioningMessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.VerticalPositioning,
                                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new VerticalPositioningErrorState(this.ParentStateMachine, this.verticalPositioningMessageData, message, this.logger));
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
