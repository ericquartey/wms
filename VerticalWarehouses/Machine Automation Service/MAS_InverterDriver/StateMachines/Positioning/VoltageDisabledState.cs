using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

//namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
//{
//    public class VoltageDisabledState : InverterStateBase
//    {
//        #region Fields

//        private const int SEND_DELAY = 50;

//        private const ushort STATUS_WORD_VALUE = 0x0050;

//        private readonly IPositioningFieldMessageData data;

//        private readonly ILogger logger;

//        private readonly ushort parameterValue;

//        private bool disposed;

//        #endregion

//        #region Constructors

//        public VoltageDisabledState(IInverterStateMachine parentStateMachine, IPositioningFieldMessageData data, ILogger logger)
//        {
//            this.logger = logger;
//            logger.LogDebug("1:Method Start");

//            this.ParentStateMachine = parentStateMachine;
//            this.data = data;

//            this.logger.LogTrace($"2:Positioning {this.data.AxisMovement} axis");

//            switch (this.data.AxisMovement)
//            {
//                case Axis.Horizontal:
//                    this.parameterValue = 0x8000;
//                    break;

//                case Axis.Vertical:
//                    this.parameterValue = 0x0000;
//                    break;
//            }

//            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, SEND_DELAY);

//            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

//            this.ParentStateMachine.EnqueueMessage(inverterMessage);

//            var messageData = new PositioningFieldMessageData(
//                this.data.AxisMovement,
//                this.data.MovementType,
//                this.data.TargetPosition,
//                this.data.TargetSpeed,
//                this.data.TargetAcceleration,
//                this.data.TargetDeceleration,
//                this.data.NumberCycles,
//                MessageVerbosity.Info);

//            var notificationMessage = new FieldNotificationMessage(
//                messageData,
//                $"{this.data.AxisMovement} Positioning started",
//                FieldMessageActor.Any,
//                FieldMessageActor.InverterDriver,
//                FieldMessageType.Positioning,
//                MessageStatus.OperationStart);

//            this.logger.LogTrace($"4:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

//            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

//            this.logger.LogDebug("5:Method End");
//        }

//        #endregion

//        #region Destructors

//        ~VoltageDisabledState()
//        {
//            this.Dispose(false);
//        }

//        #endregion

//        #region Methods

//        public override bool ProcessMessage(InverterMessage message)
//        {
//            this.logger.LogDebug("1:Method Start");
//            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

//            var returnValue = false;

//            if (message.IsError)
//            {
//                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.data, this.logger));
//            }

//            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
//            {
//                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload:X}:STATUS_WORD_VALUE={STATUS_WORD_VALUE:X}");

//                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
//                {
//                    this.ParentStateMachine.ChangeState(new PositioningModeState(this.ParentStateMachine, this.data, this.logger));

//                    returnValue = true;
//                }
//            }

//            

//            return returnValue;
//        }

//        public override void Stop()
//        {
//            this.logger.LogDebug("1:Method Start");

//            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.data, this.logger, true));

//            
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (this.disposed)
//            {
//                return;
//            }

//            if (disposing)
//            {
//            }

//            this.disposed = true;

//            base.Dispose(disposing);
//        }

//        #endregion
//    }
//}
