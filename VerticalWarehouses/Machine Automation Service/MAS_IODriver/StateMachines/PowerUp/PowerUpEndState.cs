using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    public class PowerUpEndState : IoStateBase
    {

        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        private bool disposed;

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

        #region Destructors

        ~PowerUpEndState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

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

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            //if (message.ValidOutputs && message.ElevatorMotorOn)
            //{
            //    var endNotification = new FieldNotificationMessage(
            //        null,
            //        "I/O power up complete",
            //        FieldMessageActor.Any,
            //        FieldMessageActor.IoDriver,
            //        FieldMessageType.IoPowerUp,
            //        MessageStatus.OperationEnd,
            //        ErrorLevel.NoError,
            //        (byte)this.index);

            //    this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            //    this.ParentStateMachine.PublishNotificationEvent(endNotification);
            //}
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogDebug($"1: Received Message = {message.ToString()}");

            //TEMP Check the matching between the status output flags and the message output flags (i.e. the switch ElevatorMotorON has been processed)
        }

        public override void Start()
        {
            var endNotification = new FieldNotificationMessage(
                null,
                "I/O power up complete",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.IoPowerUp,
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                (byte)this.index);

            this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
            //var resetSecurityIoMessage = new IoSHDWriteMessage();

            //resetSecurityIoMessage.SwitchElevatorMotor(true);
            //this.Logger.LogTrace($"1:Switch elevator MotorON IO={resetSecurityIoMessage}");

            //lock (this.status)
            //{
            //    this.status.UpdateOutputStates(resetSecurityIoMessage.Outputs);
            //}

            //this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);
        }

        #endregion
    }
}
