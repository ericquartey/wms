using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineVolatileDataProvider
    {
        #region Properties

        BayNumber BayTestNumber { get; set; }

        double ElevatorHorizontalPosition { get; set; }

        double ElevatorVerticalPosition { get; set; }

        double ElevatorVerticalPositionOld { get; set; }

        // Enable/disable the raw database saving on telemetry
        // (TODO: use another object for this caonfiguration parameter)
        bool EnableLocalDbSavingOnTelemetry { get; set; }

        int ExecutedCycles { get; set; }

        bool IsAutomationServiceReady { get; set; }

        Dictionary<BayNumber, bool> IsBayHomingExecuted { get; }

        Dictionary<BayNumber, bool> IsBayLightOn { get; }

        bool IsDeviceManagerBusy { get; set; }

        bool IsHomingActive { get; set; }

        bool IsHomingExecuted { get; set; }

        bool IsMachineRunning { get; }

        bool? IsOneTonMachine { get; set; }

        Dictionary<BayNumber, bool> IsShutterHomingActive { get; set; }

        Dictionary<int, int> LoadUnitsExecutedCycles { get; set; }

        List<int> LoadUnitsToTest { get; set; }

        MachinePowerState MachinePowerState { get; set; }

        MachineMode Mode { get; set; }

        int? RequiredCycles { get; set; }

        bool StopTest { get; set; }

        MachineMode UiFilteredMode { get; }

        #endregion

        #region Methods

        double GetBayEncoderPosition(BayNumber bayNumber);

        MachineMode GetMachineModeManualByBayNumber(BayNumber bayNumber);

        void SetBayEncoderPosition(BayNumber bayNumber, double position);

        #endregion
    }
}
