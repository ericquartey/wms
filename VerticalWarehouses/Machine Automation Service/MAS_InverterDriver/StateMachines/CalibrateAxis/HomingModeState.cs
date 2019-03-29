using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class HomingModeState : InverterStateBase
    {
        #region Fields

        private const ushort RESET_STATUS_WORD_VALUE = 0x0250;

        private const int sendDelay = 50;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public HomingModeState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.logger.LogTrace($"Constructor");

            this.parameterValue = 0x0006;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1-Message processed: {message.ParameterId}, {message.Payload}");
            var returnValue = false;

            if (message.IsError)
            {
                this.logger.LogTrace($"2-Change State to ErrorState");
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.logger.LogTrace($"3-Change State to ShutdownState");
                this.parentStateMachine.ChangeState(new ShutdownState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }
            if ((message.UShortPayload & RESET_STATUS_WORD_VALUE) == RESET_STATUS_WORD_VALUE)
            {
                this.logger.LogTrace($"4-Change State to EndState");
                this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }
            return returnValue;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.logger?.LogTrace($"Stop");
            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, sendDelay);
            this.parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion
    }
}
