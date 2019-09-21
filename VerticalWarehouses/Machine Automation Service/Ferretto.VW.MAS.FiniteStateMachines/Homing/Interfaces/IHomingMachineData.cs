using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces
{
    internal interface IHomingMachineData : IMachineData
    {
        #region Properties

        Axis AxisToCalibrate { get; set; }

        Axis AxisToCalibrated { get; }

        bool IsOneKMachine { get; }

        IMachineSensorsStatus MachineSensorStatus { get; }

        int MaximumSteps { get; set; }

        int NumberOfExecutedSteps { get; set; }

        #endregion
    }
}
