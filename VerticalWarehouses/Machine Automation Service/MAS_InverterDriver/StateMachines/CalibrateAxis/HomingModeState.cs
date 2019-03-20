using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class HomingModeState : InverterStateBase
    {
        #region Fields

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

            this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HomingModeState:Ctor");

            this.parameterValue = 0x0006;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            var returnValue = false;

            //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HomingModeState:ProcessMessage");
            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.parentStateMachine.ChangeState(new ShutdownState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }
            return returnValue;
        }

        #endregion
    }
}
