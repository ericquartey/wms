using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines
{
    public interface IMachineSensorsStatus
    {
        #region Properties

        decimal AxisXPosition { get; set; }

        decimal AxisYPosition { get; set; }

        bool[] DisplayedInputs { get; }

        bool IsDrawerCompletelyOffCradle { get; }

        bool IsDrawerCompletelyOnCradle { get; }

        bool IsDrawerInBay1Bottom { get; }

        bool IsDrawerInBay1Top { get; }

        bool IsDrawerInBay2Bottom { get; }

        bool IsDrawerInBay2Top { get; }

        bool IsDrawerInBay3Bottom { get; }

        bool IsDrawerInBay3Top { get; }

        bool IsDrawerPartiallyOnCradleBay1 { get; }

        bool IsMachineInEmergencyStateBay1 { get; }

        bool IsMachineInFaultState { get; }

        bool IsMachineInRunningState { get; }

        bool IsSensorZeroOnBay1 { get; }

        bool IsSensorZeroOnBay2 { get; }

        bool IsSensorZeroOnBay3 { get; }

        bool IsSensorZeroOnCradle { get; }

        bool IsSensorZeroOnElevator { get; }

        #endregion

        #region Methods

        bool IsDrawerInBayBottom(BayNumber bayNumber);

        bool IsDrawerInBayTop(BayNumber bayNumber);

        bool IsSensorZeroOnBay(BayNumber bayNumber);

        bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor);

        #endregion
    }
}
