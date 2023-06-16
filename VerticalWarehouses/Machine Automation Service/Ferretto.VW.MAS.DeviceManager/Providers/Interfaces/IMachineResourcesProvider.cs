using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IMachineResourcesProvider
    {
        #region Events

        event EventHandler<StatusUpdateEventArgs> FaultStateChanged;

        event EventHandler<StatusUpdateEventArgs> RunningStateChanged;

        event EventHandler<StatusUpdateEventArgs> SecurityStateChanged;

        #endregion

        #region Properties

        bool[] DisplayedInputs { get; }
        bool[] DisplayedOutput { get; }

        bool FireAlarm { get; }

        bool HeightAlarm { get; }

        bool IsAntiIntrusionBarrier2Bay1 { get; }

        bool IsAntiIntrusionBarrier2Bay2 { get; }

        bool IsAntiIntrusionBarrier2Bay3 { get; }

        bool IsAntiIntrusionBarrierBay1 { get; }

        bool IsAntiIntrusionBarrierBay2 { get; }

        bool IsAntiIntrusionBarrierBay3 { get; }

        bool IsDeviceManagerBusy { get; }

        bool IsDrawerCompletelyOffCradle { get; }

        bool IsDrawerCompletelyOnCradle { get; }

        bool IsDrawerInBay1Bottom { get; }

        bool IsDrawerInBay1InternalBottom { get; }

        bool IsDrawerInBay1InternalTop { get; }

        bool IsDrawerInBay1Top { get; }

        bool IsDrawerInBay2Bottom { get; }

        bool IsDrawerInBay2InternalBottom { get; }

        bool IsDrawerInBay2InternalTop { get; }

        bool IsDrawerInBay2Top { get; }

        bool IsDrawerInBay3Bottom { get; }

        bool IsDrawerInBay3InternalBottom { get; }

        bool IsDrawerInBay3InternalTop { get; }

        bool IsDrawerInBay3Top { get; }

        bool IsDrawerPartiallyOnCradle { get; }

        bool IsElevatorOverrun { get; }

        bool IsElevatorUnderrun { get; }

        bool IsMachineInEmergencyState { get; }

        bool IsMachineInFaultState { get; }

        bool IsMachineInRunningState { get; }

        bool IsMicroCarterLeftSideBay1 { get; }

        bool IsMicroCarterLeftSideBay2 { get; }

        bool IsMicroCarterLeftSideBay3 { get; }

        bool IsMicroCarterRightSideBay1 { get; }

        bool IsMicroCarterRightSideBay2 { get; }

        bool IsMicroCarterRightSideBay3 { get; }

        bool IsMushroomEmergencyButtonBay1 { get; }

        bool IsMushroomEmergencyButtonBay2 { get; }

        bool IsMushroomEmergencyButtonBay3 { get; }

        bool IsSensorZeroOnBay1 { get; }

        bool IsSensorZeroOnBay2 { get; }

        bool IsSensorZeroOnBay3 { get; }

        bool IsSensorZeroOnCradle { get; }

        bool IsSensorZeroOnElevator { get; }

        bool IsSensorZeroTopOnBay1 { get; }

        bool IsSensorZeroTopOnBay2 { get; }

        bool IsSensorZeroTopOnBay3 { get; }

        bool PreFireAlarm { get; }

        bool SensitiveCarpetsAlarm { get; }

        bool SensitiveEdgeAlarm { get; }

        bool TeleOkBay1 { get; }

        bool TeleOkBay2 { get; }

        bool TeleOkBay3 { get; }

        #endregion

        #region Methods

        void EnableNotification(bool enable);

        ShutterPosition GetShutterPosition(InverterIndex inverterIndex);

        bool IsDrawerInBayBottom(BayNumber bayNumber);

        bool IsDrawerInBayBottom(BayNumber bayNumber, bool isExternalDouble);

        bool IsDrawerInBayExternalPosition(BayNumber bayNumber, bool isExternalDoubleBay);

        bool IsDrawerInBayInternalBottom(BayNumber bayNumber);

        bool IsDrawerInBayInternalPosition(BayNumber bayNumber, bool isDouble);

        bool IsDrawerInBayInternalTop(BayNumber bayNumber);

        bool IsDrawerInBayTop(BayNumber bayNumber);

        bool IsDrawerInBayTop(BayNumber bayNumber, bool isExternalDouble);

        bool IsProfileCalibratedBay(BayNumber bayNumber);

        bool IsSensorZeroOnBay(BayNumber bayNumber);

        bool IsSensorZeroTopOnBay(BayNumber bayNumber);

        void OnFaultStateChanged(StatusUpdateEventArgs e);

        bool UpdateDiagOutCurrent(byte ioIndex, int[] newOutCurrent);

        bool UpdateDiagOutFault(byte ioIndex, bool[] newOutFault);

        bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor);

        #endregion
    }
}
