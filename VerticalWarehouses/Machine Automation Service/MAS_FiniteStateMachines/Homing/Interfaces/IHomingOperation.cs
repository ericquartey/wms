using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces
{
    public interface IHomingOperation
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        Axis AxisToCalibrated { get; }

        int MaximumSteps { get; }

        int NumberOfExecutedSteps { get; }

        #endregion
    }
}
