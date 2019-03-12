using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class ShutdownState : InverterStateBase
    {
        #region Fields

        private const ushort StatusWordValue = 0x0031;

        private readonly Axis axisToCalibrate;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public ShutdownState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - ShutdownState:Ctor");
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

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
                new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            bool returnValue = false;

            Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - ShutdownState:ProcessMessage");
            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                if ((message.UShortPayload & StatusWordValue) == StatusWordValue)
                {
                    this.parentStateMachine.ChangeState(new SwitchOnState(this.parentStateMachine, this.axisToCalibrate));
                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
