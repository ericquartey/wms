using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineErrorCode
    {
        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(NoError))]
        NoError = -1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCompletelyLoaded), (int)MachineErrorSeverity.Normal)]
        CradleNotCompletelyLoaded = 1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForPositioning), (int)MachineErrorSeverity.Low)]
        [ErrorCondition(typeof(IElevatorHorizontalChainZeroConditionEvaluator))]
        ConditionsNotMetForPositioning,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForRunning), (int)MachineErrorSeverity.Low)]
        //[ErrorCondition(typeof(ISecurityIsClearedConditionEvaluator))]
        ConditionsNotMetForRunning,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForHoming), (int)MachineErrorSeverity.Low)]
        [ErrorCondition(typeof(IElevatorHorizontalChainZeroConditionEvaluator))]
        ConditionsNotMetForHoming,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityWasTriggered), (int)MachineErrorSeverity.High)]
        //[ErrorCondition(typeof(ISecurityIsClearedConditionEvaluator))]
        SecurityWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityButtonWasTriggered), (int)MachineErrorSeverity.High)]
        SecurityButtonWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityBarrierWasTriggered), (int)MachineErrorSeverity.High)]
        SecurityBarrierWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityLeftSensorWasTriggered), (int)MachineErrorSeverity.High)]
        SecurityLeftSensorWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterFaultStateDetected), (int)MachineErrorSeverity.High)]
        InverterFaultStateDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyLoadedDuringPickup), (int)MachineErrorSeverity.Normal)]
        CradleNotCorrectlyLoadedDuringPickup,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyUnloadedDuringDeposit), (int)MachineErrorSeverity.Normal)]
        CradleNotCorrectlyUnloadedDuringDeposit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterPickup), (int)MachineErrorSeverity.Normal)]
        ZeroSensorErrorAfterPickup,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterDeposit), (int)MachineErrorSeverity.Normal)]
        ZeroSensorErrorAfterDeposit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidPresenceSensors), (int)MachineErrorSeverity.Normal)]
        InvalidPresenceSensors,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MissingZeroSensorWithEmptyElevator), (int)MachineErrorSeverity.Normal)]
        MissingZeroSensorWithEmptyElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorActiveWithFullElevator), (int)MachineErrorSeverity.Normal)]
        ZeroSensorActiveWithFullElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitPresentOnEmptyElevator), (int)MachineErrorSeverity.Normal)]
        LoadUnitPresentOnEmptyElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TopLevelBayOccupied), (int)MachineErrorSeverity.Normal)]
        TopLevelBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BottomLevelBayOccupied), (int)MachineErrorSeverity.Normal)]
        BottomLevelBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TopLevelBayEmpty), (int)MachineErrorSeverity.Normal)]
        TopLevelBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BottomLevelBayEmpty), (int)MachineErrorSeverity.Normal)]
        BottomLevelBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SensorZeroBayNotActiveAtStart), (int)MachineErrorSeverity.Normal)]
        SensorZeroBayNotActiveAtStart,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterConnectionError), (int)MachineErrorSeverity.High)]
        InverterConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceConnectionError), (int)MachineErrorSeverity.High)]
        IoDeviceConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LaserConnectionError), (int)MachineErrorSeverity.Low)]
        LaserConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitWeightExceeded), (int)MachineErrorSeverity.Normal)]
        LoadUnitWeightExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitHeightFromBayExceeded), (int)MachineErrorSeverity.Normal)]
        LoadUnitHeightFromBayExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitHeightToBayExceeded), (int)MachineErrorSeverity.Normal)]
        LoadUnitHeightToBayExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitWeightTooLow), (int)MachineErrorSeverity.Normal)]
        LoadUnitWeightTooLow,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineWeightExceeded), (int)MachineErrorSeverity.Normal)]
        MachineWeightExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationBelowLowerBound), (int)MachineErrorSeverity.Normal)]
        DestinationBelowLowerBound,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationOverUpperBound), (int)MachineErrorSeverity.Normal)]
        DestinationOverUpperBound,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BayInvertersBusy), (int)MachineErrorSeverity.Normal)]
        BayInvertersBusy,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceError), (int)MachineErrorSeverity.Normal)]
        IoDeviceError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineModeNotValid), (int)MachineErrorSeverity.Normal)]
        MachineModeNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AnotherMissionIsActiveForThisLoadUnit), (int)MachineErrorSeverity.Normal)]
        AnotherMissionIsActiveForThisLoadUnit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AnotherMissionIsActiveForThisBay), (int)MachineErrorSeverity.Normal)]
        AnotherMissionIsActiveForThisBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AnotherMissionOfThisTypeIsActive), (int)MachineErrorSeverity.Normal)]
        AnotherMissionOfThisTypeIsActive,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(WarehouseIsFull), (int)MachineErrorSeverity.Normal)]
        WarehouseIsFull,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CellLogicallyOccupied), (int)MachineErrorSeverity.Normal)]
        CellLogicallyOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MoveBayChainNotAllowed), (int)MachineErrorSeverity.Normal)]
        MoveBayChainNotAllowed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AutomaticRestoreNotAllowed), (int)MachineErrorSeverity.Normal)]
        AutomaticRestoreNotAllowed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationTypeNotValid), (int)MachineErrorSeverity.Normal)]
        DestinationTypeNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MissionTypeNotValid), (int)MachineErrorSeverity.Normal)]
        MissionTypeNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ResumeCommandNotValid), (int)MachineErrorSeverity.Normal)]
        ResumeCommandNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationBayNotCalibrated), (int)MachineErrorSeverity.Normal)]
        DestinationBayNotCalibrated,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(NoLoadUnitInSource), (int)MachineErrorSeverity.Normal)]
        NoLoadUnitInSource,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceDb), (int)MachineErrorSeverity.Normal)]
        LoadUnitSourceDb,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitDestinationCell), (int)MachineErrorSeverity.Normal)]
        LoadUnitDestinationCell,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitElevator), (int)MachineErrorSeverity.Normal)]
        LoadUnitElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitNotRemoved), (int)MachineErrorSeverity.Normal)]
        LoadUnitNotRemoved,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitDestinationBay), (int)MachineErrorSeverity.Normal)]
        LoadUnitDestinationBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceCell), (int)MachineErrorSeverity.Normal)]
        LoadUnitSourceCell,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitNotFound), (int)MachineErrorSeverity.Normal)]
        LoadUnitNotFound,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitNotLoaded), (int)MachineErrorSeverity.Normal)]
        LoadUnitNotLoaded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceBay), (int)MachineErrorSeverity.Normal)]
        LoadUnitSourceBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitShutterOpen), (int)MachineErrorSeverity.Normal)]
        LoadUnitShutterOpen,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitShutterInvalid), (int)MachineErrorSeverity.Normal)]
        LoadUnitShutterInvalid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitShutterClosed), (int)MachineErrorSeverity.Normal)]
        LoadUnitShutterClosed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitPresentInCell), (int)MachineErrorSeverity.Normal)]
        LoadUnitPresentInCell,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitOtherBay), (int)MachineErrorSeverity.Normal)]
        LoadUnitOtherBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceElevator), (int)MachineErrorSeverity.Normal)]
        LoadUnitSourceElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitMissingOnElevator), (int)MachineErrorSeverity.Normal)]
        LoadUnitMissingOnElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitMissingOnBay), (int)MachineErrorSeverity.Normal)]
        LoadUnitMissingOnBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitUndefinedUpper), (int)MachineErrorSeverity.Normal)]
        LoadUnitUndefinedUpper,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitUndefinedBottom), (int)MachineErrorSeverity.Normal)]
        LoadUnitUndefinedBottom,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(FirstTestFailed), (int)MachineErrorSeverity.Normal)]
        FirstTestFailed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(FullTestFailed), (int)MachineErrorSeverity.Normal)]
        FullTestFailed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(WarehouseNotEmpty), (int)MachineErrorSeverity.Normal)]
        WarehouseNotEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SensorZeroBayNotActiveAtEnd), (int)MachineErrorSeverity.Normal)]
        SensorZeroBayNotActiveAtEnd,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityRightSensorWasTriggered), (int)MachineErrorSeverity.High)]
        SecurityRightSensorWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(VerticalPositionChanged), (int)MachineErrorSeverity.Normal)]
        VerticalPositionChanged,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidBay), (int)MachineErrorSeverity.Normal)]
        InvalidBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidPositionBay), (int)MachineErrorSeverity.Normal)]
        InvalidPositionBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ElevatorOverrunDetected), (int)MachineErrorSeverity.High)]
        [ErrorCondition(typeof(IElevatorOverrunConditionEvaluator))]
        ElevatorOverrunDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ElevatorUnderrunDetected), (int)MachineErrorSeverity.High)]
        [ErrorCondition(typeof(IElevatorUnderrunConditionEvaluator))]
        ElevatorUnderrunDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ExternalBayEmpty), (int)MachineErrorSeverity.Normal)]
        ExternalBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ExternalBayOccupied), (int)MachineErrorSeverity.Normal)]
        ExternalBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(WmsError), (int)MachineErrorSeverity.Normal)]
        WmsError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BayPositionDisabled), (int)MachineErrorSeverity.Normal)]
        BayPositionDisabled,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MoveExtBayNotAllowed), (int)MachineErrorSeverity.Normal)]
        MoveExtBayNotAllowed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(StartPositioningBlocked), (int)MachineErrorSeverity.Normal)]
        StartPositioningBlocked,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterCommandTimeout), (int)MachineErrorSeverity.Normal)]
        InverterCommandTimeout,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceCommandTimeout), (int)MachineErrorSeverity.Low)]
        IoDeviceCommandTimeout,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TelescopicBayError), (int)MachineErrorSeverity.Normal)]
        [ErrorCondition(typeof(IBayTelescopicZeroConditionEvaluator))]
        TelescopicBayError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitTareError), (int)MachineErrorSeverity.Normal)]
        LoadUnitTareError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(VerticalZeroLowError), (int)MachineErrorSeverity.NeedsHoming)]
        VerticalZeroLowError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(VerticalZeroHighError), (int)MachineErrorSeverity.NeedsHoming)]
        VerticalZeroHighError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitHeightFromBayTooLow), (int)MachineErrorSeverity.Normal)]
        LoadUnitHeightFromBayTooLow,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(PreFireAlarm), (int)MachineErrorSeverity.High)]
        [ErrorCondition(typeof(IPreFireAllarmConditionEvaluator))]
        PreFireAlarm,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(FireAlarm), (int)MachineErrorSeverity.High)]
        [ErrorCondition(typeof(IFireAllarmConditionEvaluator))]
        FireAlarm,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorBaseCode), (int)MachineErrorSeverity.Normal)]
        InverterErrorBaseCode = 1000,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorInvalidParameter), (int)MachineErrorSeverity.Normal)]
        InverterErrorInvalidParameter = InverterErrorBaseCode + 1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorInvalidDataset), (int)MachineErrorSeverity.Normal)]
        InverterErrorInvalidDataset = InverterErrorBaseCode + 2,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorParameterIsWriteOnly), (int)MachineErrorSeverity.Normal)]
        InverterErrorParameterIsWriteOnly = InverterErrorBaseCode + 3,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorParameterIsReadOnly), (int)MachineErrorSeverity.Normal)]
        InverterErrorParameterIsReadOnly = InverterErrorBaseCode + 4,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorEepromReadError), (int)MachineErrorSeverity.Normal)]
        InverterErrorEepromReadError = InverterErrorBaseCode + 5,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorEepromWriteError), (int)MachineErrorSeverity.Normal)]
        InverterErrorEepromWriteError = InverterErrorBaseCode + 6,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorEepromChecksumError), (int)MachineErrorSeverity.Normal)]
        InverterErrorEepromChecksumError = InverterErrorBaseCode + 7,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorCannotWriteParameterWhileRunning), (int)MachineErrorSeverity.Normal)]
        InverterErrorCannotWriteParameterWhileRunning = InverterErrorBaseCode + 8,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorDatasetValuesAreDifferent), (int)MachineErrorSeverity.Normal)]
        InverterErrorDatasetValuesAreDifferent = InverterErrorBaseCode + 9,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorUnknownParameter), (int)MachineErrorSeverity.Normal)]
        InverterErrorUnknownParameter = InverterErrorBaseCode + 11,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorSyntaxError), (int)MachineErrorSeverity.Normal)]
        InverterErrorSyntaxError = InverterErrorBaseCode + 13,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorWrongPayloadLength), (int)MachineErrorSeverity.Normal)]
        InverterErrorWrongPayloadLength = InverterErrorBaseCode + 14,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorNodeNotAvailable), (int)MachineErrorSeverity.Normal)]
        InverterErrorNodeNotAvailable = InverterErrorBaseCode + 20,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorSyntaxError), (int)MachineErrorSeverity.Normal)]
        InverterErrorSyntaxError2 = InverterErrorBaseCode + 30,
    }
}
