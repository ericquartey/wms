using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Calibrate
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
