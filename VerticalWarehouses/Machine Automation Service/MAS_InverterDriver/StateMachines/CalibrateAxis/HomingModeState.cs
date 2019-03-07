using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class HomingModeState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly short parameterValue;

        #endregion

        #region Constructors

        public HomingModeState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            Console.WriteLine("HomingModeState");
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            this.parameterValue = 0x0006;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(InverterMessage message)
        {
            Console.WriteLine("HomingModeState-ProcessMessage");
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate));

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
                if (message.ShortPayload == this.parameterValue)
                    this.parentStateMachine.ChangeState(
                        new ShutdownState(this.parentStateMachine, this.axisToCalibrate));
        }

        #endregion
    }
}
