using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

//namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
//{
//    public class VoltageDisabledState : InverterStateBase
//    {
//        #region Fields

//        private const int SEND_DELAY = 50;

//        private const ushort STATUS_WORD_VALUE = 0x0050;

//        private readonly ILogger logger;

//        private IShutterPositioningFieldMessageData data;

//        private bool disposed;

//        #endregion

//        #region Constructors

//        public VoltageDisabledState(IInverterStateMachine parentStateMachine, IShutterPositioningFieldMessageData data, ILogger logger)
//        {
//            this.logger = logger;
//            this.logger.LogDebug("1:Method Start");

//            this.ParentStateMachine = parentStateMachine;
//            this.data = data;

//            this.logger.LogTrace($"2:Positioning shutter to {this.data.ShutterPosition} position");

//            ushort parameterValue = 0x0000;

//            var inverterMessage = new InverterMessage(this.data.SystemIndex, (short)InverterParameterId.ControlWordParam, parameterValue, SEND_DELAY);

//            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

//            this.ParentStateMachine.EnqueueMessage(inverterMessage);

//            var messageData = new ShutterPositioningFieldMessageData(
//                this.data.ShutterPosition,
//                this.data.SystemIndex,
//                MessageVerbosity.Info);

//            var notificationMessage = new FieldNotificationMessage(
//                messageData,
//                $"Positioning shutter {this.data.SystemIndex} to {this.data.ShutterPosition} position started",
//                FieldMessageActor.Any,
//                FieldMessageActor.InverterDriver,
//                FieldMessageType.ShutterPositioning,
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
//                    this.ParentStateMachine.ChangeState(new ShutterPositioningModeState(this.ParentStateMachine, this.data, this.logger));

//                    returnValue = true;
//                }
//            }

//            this.logger.LogDebug("4:Method End");

//            return returnValue;
//        }

//        public override void Stop()
//        {
//            this.logger.LogDebug("1:Method Start");

//            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.data, this.logger, true));

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
