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
    public class ShutdownState : InverterStateBase
    {
        #region Fields

        private const ushort RESET_STATUS_WORD_VALUE = 0x0250;

        private const int sendDelay = 50;

        private const ushort StatusWordValue = 0x0031;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public ShutdownState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.logger.LogTrace($"Constructor");

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x8006;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x0006;
                    break;
            }

            var inverterMessage =
                new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, sendDelay);

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

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                if ((message.UShortPayload & StatusWordValue) == StatusWordValue)
                {
                    this.logger.LogTrace($"3-Change State to SwitchOnState");
                    this.parentStateMachine.ChangeState(new SwitchOnState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }

                if ((message.UShortPayload & RESET_STATUS_WORD_VALUE) == RESET_STATUS_WORD_VALUE)
                {
                    this.logger.LogTrace($"4-Change State to EndState");
                    this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }
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
