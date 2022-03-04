using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.Reset
{
    internal sealed class ResetEndState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public ResetEndState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            IoIndex index,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}");

            // TEMP Check the matching between the status output flags and the message output flags (i.e. the switch ElevatorMotorON has been processed)
            if (this.status.MatchOutputs(message.Outputs))
            {
                this.Logger.LogTrace("End State State ProcessMessage Notification Event");
                var endNotification = new FieldNotificationMessage(
                    null,
                    "IO Reset complete",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.IoReset,
                    MessageStatus.OperationEnd,
                    (byte)this.index);

                this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void Start()
        {
            var resetSecurityIoMessage = new IoWriteMessage { PowerEnable = true };

            resetSecurityIoMessage.SwitchElevatorMotor(true);

            this.Logger.LogDebug($"1:Switch elevator MotorON IO={resetSecurityIoMessage}");
            lock (this.status)
            {
                this.status.UpdateOutputStates(resetSecurityIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);
        }

        #endregion
    }
}
