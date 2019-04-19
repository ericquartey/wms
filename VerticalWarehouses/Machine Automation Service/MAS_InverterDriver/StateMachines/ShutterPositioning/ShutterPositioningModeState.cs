using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningModeState : InverterStateBase
    {
        #region Fields

        private const int SEND_DELAY = 50;

        private readonly IShutterPositioningFieldMessageData data;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningModeState(IInverterStateMachine parentStateMachine, IShutterPositioningFieldMessageData data, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.data = data;

            this.logger.LogTrace($"2:Positioning shutter to {this.data.ShutterPosition} position");

            ushort parameterValue = 0x0002; // INFO Velocity mode. Bonfiglioli's documentation (pp 69 and following) says it should be the default (and only for agl inverters) value on start up

            var inverterMessage = new InverterMessage(this.data.SystemIndex, (short)InverterParameterId.SetOperatingModeParam, parameterValue, SEND_DELAY);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("4:Method End");
        }

        #endregion

        #region Destructors

        ~ShutterPositioningModeState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.data, this.logger));
            }

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.ParentStateMachine.ChangeState(new ShutdownState(this.ParentStateMachine, this.data, this.logger));
                returnValue = true;
            }

            this.logger.LogDebug("3:Method End");

            return returnValue;
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.data, this.logger, true));

            this.logger.LogDebug("2:Method End");
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
