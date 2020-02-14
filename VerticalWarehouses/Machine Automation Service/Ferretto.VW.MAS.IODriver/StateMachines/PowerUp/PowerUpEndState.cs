using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    internal sealed class PowerUpEndState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public PowerUpEndState(
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
            this.Logger.LogTrace($"1: Received Message = {message.ToString()}");

            // TEMP Check the matching between the status output flags and the message output flags (i.e. the switch ElevatorMotorON has been processed)
        }

        public override void Start()
        {
            var endNotification = new FieldNotificationMessage(
                null,
                "I/O power up complete",
                FieldMessageActor.IoDriver,
                FieldMessageActor.IoDriver,
                FieldMessageType.IoPowerUp,
                MessageStatus.OperationEnd,
                (byte)this.index);

            this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);

            // why is this code commented out?
            /*
             var resetSecurityIoMessage = new IoSHDWriteMessage();
             resetSecurityIoMessage.SwitchElevatorMotor(true);
             this.Logger.LogTrace($"1:Switch elevator MotorON IO={resetSecurityIoMessage}");

             lock (this.status)
             {
                this.status.UpdateOutputStates(resetSecurityIoMessage.Outputs);
             }

             this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);
            */
        }

        #endregion
    }
}
