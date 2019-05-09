using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

//namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
//{
//    public class EndState : InverterStateBase
//    {
//        #region Fields

//        private const int SEND_DELAY = 50;

//        private const ushort STATUS_WORD_VALUE = 0x0250;

//        private readonly IShutterPositioningFieldMessageData data;

//        private readonly ILogger logger;

//        private bool disposed;

//        #endregion

//        #region Constructors

//        public EndState(IInverterStateMachine parentStateMachine, IShutterPositioningFieldMessageData data, ILogger logger, bool stopRequested = false)
//        {
//            this.logger = logger;
//            this.logger.LogDebug("1:Method Start");

//            this.ParentStateMachine = parentStateMachine;
//            this.data = data;

//            if (stopRequested)
//            {
//                var stopParameterValue = 0x0007; // INFO to execute a secure stop, we go back to the previous Inverter state (SwitchOn)

//                var inverterMessage = new InverterMessage(this.data.SystemIndex, (short)InverterParameterId.ControlWordParam, (ushort)stopParameterValue, SEND_DELAY);

//                this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

//                this.ParentStateMachine.EnqueueMessage(inverterMessage);
//            }
//            else
//            {
//                var messageData = new ShutterPositioningFieldMessageData(
//                    this.data.ShutterPosition,
//                    this.data.SystemIndex);
//                var endNotification = new FieldNotificationMessage(messageData,
//                    "Shutter positioning complete",
//                    FieldMessageActor.Any,
//                    FieldMessageActor.InverterDriver,
//                    FieldMessageType.ShutterPositioning,
//                    MessageStatus.OperationEnd);

//                this.logger.LogTrace(
//                    $"3:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

//                this.ParentStateMachine.PublishNotificationEvent(endNotification);
//            }
//            this.logger.LogDebug("4:Method End");
//        }

//        #endregion

//        #region Methods

//        public override bool ProcessMessage(InverterMessage message)
//        {
//            var returnValue = false;

//            this.logger.LogDebug("1:Method Start");
//            this.logger.LogTrace($"2:Process Inverter Message: {message}: requested position={this.data.ShutterPosition}");

//            if (message.IsError)
//            {
//                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.data, this.logger));
//            }

//            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
//            {
//                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload:X}:RESET_STATUS_WORD_VALUE={STATUS_WORD_VALUE:X}");

//                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
//                {
//                    var messageData = new ShutterPositioningFieldMessageData(
//                        this.data.ShutterPosition,
//                    this.data.SystemIndex
//                    );
//                    var endNotification = new FieldNotificationMessage(messageData,
//                        $"{this.data.ShutterPosition} positioning complete",
//                        FieldMessageActor.Any,
//                        FieldMessageActor.InverterDriver,
//                        FieldMessageType.ShutterPositioning,
//                        MessageStatus.OperationStop);

//                    this.logger.LogTrace(
//                        $"4:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

//                    this.ParentStateMachine.PublishNotificationEvent(endNotification);

//                    returnValue = true;
//                }
//            }

//            this.logger.LogDebug("5:Method End");

//            return returnValue;
//        }

//        public override void Stop()
//        {
//            this.logger.LogDebug("1:Method Start");
//            this.logger.LogDebug("2:Method End");
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
