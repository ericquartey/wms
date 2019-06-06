using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningEndState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly int numberExecutedSteps;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningEndState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger,
            int numberExecutedSteps, bool stopRequested = false)
        {
            logger?.LogTrace("1:Method Start");

            this.logger = logger;
            this.stopRequested = stopRequested;
            this.ParentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.numberExecutedSteps = numberExecutedSteps;
        }

        #endregion

        #region Destructors

        ~PositioningEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            var notificationMessage = new NotificationMessage(
                               this.positioningMessageData,
                               this.positioningMessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                               MessageActor.Any,
                               MessageActor.FiniteStateMachines,
                               MessageType.Positioning,
                               MessageStatus.OperationStop);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.logger?.LogTrace("1:Method Start");

            if (this.stopRequested)
            {
                //TEMP The FSM must be defined the inverter to stop (by the inverter index)
                var data = new InverterStopFieldMessageData(InverterIndex.MainInverter);

                var stopMessage = new FieldCommandMessage(data,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                var notificationMessage = new NotificationMessage(
                    this.positioningMessageData,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationEnd);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");
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
