using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

//namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
//{
//    public class PositioningModeState : InverterStateBase
//    {
//        #region Fields

//        private const int SEND_DELAY = 50;

//        private readonly IPositioningFieldMessageData data;

//        private readonly ILogger logger;

//        private bool disposed;

//        #endregion

//        #region Constructors

//        public PositioningModeState(IInverterStateMachine parentStateMachine, IPositioningFieldMessageData data, ILogger logger)
//        {
//            this.logger = logger;
//            this.logger.LogDebug("1:Method Start");

//            this.ParentStateMachine = parentStateMachine;
//            this.data = data;

//            ushort parameterValue = 0x0001;

//            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, parameterValue, SEND_DELAY);

//            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

//            parentStateMachine.EnqueueMessage(inverterMessage);

//            
//        }

//        #endregion

//        #region Destructors

//        ~PositioningModeState()
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

//            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
//            {
//                this.ParentStateMachine.ChangeState(new ShutdownState(this.ParentStateMachine, this.data, this.logger));
//                returnValue = true;
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
