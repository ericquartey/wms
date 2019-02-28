using System;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.InverterDriver.StateMachines.Calibrate
{
    public class VoltageDisabledState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public VoltageDisabledState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x8000;
                    break;
            }

            var inverterMessage =
                new InverterMessage(0x00, (short) InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate));

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                    this.parentStateMachine.ChangeState(new HomingModeState(this.parentStateMachine,
                        this.axisToCalibrate));
        }

        #endregion
    }
}
