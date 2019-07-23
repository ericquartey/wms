using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.Interfaces
{
    public interface IMachineSensorsStatus
    {
        #region Properties

        bool[] DisplayedInputs { get; }

        bool IsDrawerCompletelyOffCradle { get; }

        bool IsDrawerCompletelyOnCradle { get; }

        bool IsDrawerInBay1Up { get; }

        bool IsDrawerPartiallyOnCradle { get; }

        bool IsMachineInEmergencyState { get; }

        bool IsSensorZeroOnCradle { get; }

        bool IsSensorZeroOnElevator { get; }

        #endregion

        #region Methods

        bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor);

        #endregion
    }
}
