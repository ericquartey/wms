using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.Interface
{
    public interface IMachineSensorsStatus
    {


        #region Properties

        decimal AxisXPosition { get; set; }

        decimal AxisYPosition { get; set; }

        bool[] DisplayedInputs { get; }

        bool IsDrawerCompletelyOffCradle { get; }

        bool IsDrawerCompletelyOnCradle { get; }

        bool IsDrawerInBay1Up { get; }

        bool IsDrawerPartiallyOnCradleBay1 { get; }

        bool IsMachineInEmergencyStateBay1 { get; }

        bool IsMachineInFaultState { get; }

        bool IsMachineInRunningState { get; }

        bool IsSensorZeroOnCradle { get; }

        bool IsSensorZeroOnElevator { get; }

        #endregion



        #region Methods

        bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor);

        #endregion
    }
}
