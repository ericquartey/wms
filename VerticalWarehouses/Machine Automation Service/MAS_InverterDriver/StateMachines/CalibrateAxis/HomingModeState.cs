using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class HomingModeState : InverterStateBase
    {
        #region Fields

        private const int SEND_DELAY = 50;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingModeState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            ushort parameterValue = 0x0006;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, parameterValue, SEND_DELAY);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~HomingModeState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.ParentStateMachine.ChangeState(new ShutdownState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }

            this.logger.LogDebug("4:Method End");

            return returnValue;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.axisToCalibrate, this.logger, true));

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
