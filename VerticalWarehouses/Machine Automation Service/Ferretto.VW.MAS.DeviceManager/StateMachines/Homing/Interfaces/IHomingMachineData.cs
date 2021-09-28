using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.Homing.Interfaces
{
    internal interface IHomingMachineData : IMachineData
    {
        #region Properties

        Axis AxisToCalibrate { get; set; }

        Calibration CalibrationType { get; set; }

        InverterIndex CurrentInverterIndex { get; set; }

        InverterIndex InverterIndexOld { get; set; }

        bool IsOneTonMachine { get; }

        int? LoadingUnitId { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        int MaximumSteps { get; set; }

        int NumberOfExecutedSteps { get; set; }

        Axis RequestedAxisToCalibrate { get; set; }

        bool ShowErrors { get; }

        bool TurnBack { get; }

        double VerticalStartingPosition { get; set; }

        #endregion
    }
}
