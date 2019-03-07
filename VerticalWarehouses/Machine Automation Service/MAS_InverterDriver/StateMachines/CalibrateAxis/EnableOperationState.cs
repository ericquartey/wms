using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class EnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public EnableOperationState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            Console.WriteLine("EnableOperationState");
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x800F;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x000F;
                    break;
            }

            var inverterMessage =
                new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(InverterMessage message)
        {
            Console.WriteLine("EnableOperationState-ProcessMessage");
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate));

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                {
                    this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate));
                    ;
                }
        }

        #endregion
    }
}
