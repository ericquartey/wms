﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    internal interface IMachineResourcesProvider
    {
        #region Events

        event EventHandler<StatusUpdateEventArgs> FaultStateChanged;

        event EventHandler<StatusUpdateEventArgs> RunningStateChanged;

        #endregion

        #region Properties

        bool[] DisplayedInputs { get; }

        bool IsAntiIntrusionBarrierBay1 { get; }

        bool IsAntiIntrusionBarrierBay2 { get; }

        bool IsAntiIntrusionBarrierBay3 { get; }

        bool IsDrawerCompletelyOffCradle { get; }

        bool IsDrawerCompletelyOnCradle { get; }

        bool IsDrawerInBay1Bottom { get; }

        bool IsDrawerInBay1Top { get; }

        bool IsDrawerInBay2Bottom { get; }

        bool IsDrawerInBay2Top { get; }

        bool IsDrawerInBay3Bottom { get; }

        bool IsDrawerInBay3Top { get; }

        bool IsDrawerPartiallyOnCradle { get; }

        bool IsMachineInEmergencyState { get; }

        bool IsMachineInFaultState { get; }

        bool IsMachineInRunningState { get; }

        bool IsMicroCarterLeftSide { get; }

        bool IsMicroCarterRightSide { get; }

        bool IsMushroomEmergencyButtonBay1 { get; }

        bool IsMushroomEmergencyButtonBay2 { get; }

        bool IsMushroomEmergencyButtonBay3 { get; }

        bool IsSensorZeroOnBay1 { get; }

        bool IsSensorZeroOnBay2 { get; }

        bool IsSensorZeroOnBay3 { get; }

        bool IsSensorZeroOnCradle { get; }

        bool IsSensorZeroOnElevator { get; }

        #endregion

        #region Methods

        void EnableNotification(bool enable);

        ShutterPosition GetShutterPosition(InverterIndex inverterIndex);

        bool IsDrawerInBayBottom(BayNumber bayNumber);

        bool IsDrawerInBayTop(BayNumber bayNumber);

        bool IsProfileCalibratedBay(BayNumber bayNumber);

        bool IsSensorZeroOnBay(BayNumber bayNumber);

        void OnFaultStateChanged(StatusUpdateEventArgs e);

        bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor);

        #endregion
    }
}
