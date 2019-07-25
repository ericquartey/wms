using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.Interfaces
{
    public interface IMachineSensorsStatus
    {
        #region Properties

        bool[] DisplayedInputs { get; }

        bool DrawerIsCompletelyOnCradle { get; }

        bool DrawerIsPartiallyOnCradle { get; }

        bool IsDrawerInBay1Up { get; }

        bool IsSensorZeroOnCradle { get; }

        bool IsSensorZeroOnElevator { get; }

        bool MachineIsInEmergencyState { get; }

        #endregion

        #region Methods

        bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor);

        #endregion
    }
}
