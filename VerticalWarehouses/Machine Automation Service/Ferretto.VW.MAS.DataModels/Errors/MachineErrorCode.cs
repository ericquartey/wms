using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineErrorCode
    {
        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(NoError))]
        NoError = -1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCompletelyLoaded), 1)]
        CradleNotCompletelyLoaded = 1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForPositioning), 0)]
        [ErrorCondition(typeof(IElevatorHorizontalChainZeroConditionEvaluator))]
        ConditionsNotMetForPositioning,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForRunning), 0)]
        //[ErrorCondition(typeof(ISecurityIsClearedConditionEvaluator))]
        ConditionsNotMetForRunning,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForHoming), 0)]
        [ErrorCondition(typeof(IElevatorHorizontalChainZeroConditionEvaluator))]
        ConditionsNotMetForHoming,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityWasTriggered), 2)]
        //[ErrorCondition(typeof(ISecurityIsClearedConditionEvaluator))]
        SecurityWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityButtonWasTriggered), 2)]
        SecurityButtonWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityBarrierWasTriggered), 2)]
        SecurityBarrierWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityLeftSensorWasTriggered), 2)]
        SecurityLeftSensorWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterFaultStateDetected), 2)]
        InverterFaultStateDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyLoadedDuringPickup), 1)]
        CradleNotCorrectlyLoadedDuringPickup,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyUnloadedDuringDeposit), 1)]
        CradleNotCorrectlyUnloadedDuringDeposit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterPickup), 1)]
        ZeroSensorErrorAfterPickup,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterDeposit), 1)]
        ZeroSensorErrorAfterDeposit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidPresenceSensors), 1)]
        InvalidPresenceSensors,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MissingZeroSensorWithEmptyElevator), 1)]
        MissingZeroSensorWithEmptyElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorActiveWithFullElevator), 1)]
        ZeroSensorActiveWithFullElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitPresentOnEmptyElevator), 1)]
        LoadUnitPresentOnEmptyElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TopLevelBayOccupied), 1)]
        TopLevelBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BottomLevelBayOccupied), 1)]
        BottomLevelBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TopLevelBayEmpty), 1)]
        TopLevelBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BottomLevelBayEmpty), 1)]
        BottomLevelBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SensorZeroBayNotActiveAtStart), 1)]
        SensorZeroBayNotActiveAtStart,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterConnectionError), 2)]
        InverterConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceConnectionError), 2)]
        IoDeviceConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LaserConnectionError), 0)]
        LaserConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitWeightExceeded), 1)]
        LoadUnitWeightExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitHeightFromBayExceeded), 1)]
        LoadUnitHeightFromBayExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitHeightToBayExceeded), 1)]
        LoadUnitHeightToBayExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitWeightTooLow), 1)]
        LoadUnitWeightTooLow,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineWeightExceeded), 1)]
        MachineWeightExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationBelowLowerBound), 1)]
        DestinationBelowLowerBound,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationOverUpperBound), 1)]
        DestinationOverUpperBound,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BayInvertersBusy), 1)]
        BayInvertersBusy,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceError), 1)]
        IoDeviceError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineModeNotValid), 1)]
        MachineModeNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AnotherMissionIsActiveForThisLoadUnit), 1)]
        AnotherMissionIsActiveForThisLoadUnit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AnotherMissionIsActiveForThisBay), 1)]
        AnotherMissionIsActiveForThisBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AnotherMissionOfThisTypeIsActive), 1)]
        AnotherMissionOfThisTypeIsActive,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(WarehouseIsFull), 1)]
        WarehouseIsFull,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CellLogicallyOccupied), 1)]
        CellLogicallyOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MoveBayChainNotAllowed), 1)]
        MoveBayChainNotAllowed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(AutomaticRestoreNotAllowed), 1)]
        AutomaticRestoreNotAllowed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationTypeNotValid), 1)]
        DestinationTypeNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MissionTypeNotValid), 1)]
        MissionTypeNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ResumeCommandNotValid), 1)]
        ResumeCommandNotValid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(DestinationBayNotCalibrated), 1)]
        DestinationBayNotCalibrated,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(NoLoadUnitInSource), 1)]
        NoLoadUnitInSource,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceDb), 1)]
        LoadUnitSourceDb,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitDestinationCell), 1)]
        LoadUnitDestinationCell,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitElevator), 1)]
        LoadUnitElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitNotRemoved), 1)]
        LoadUnitNotRemoved,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitDestinationBay), 1)]
        LoadUnitDestinationBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceCell), 1)]
        LoadUnitSourceCell,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitNotFound), 1)]
        LoadUnitNotFound,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitNotLoaded), 1)]
        LoadUnitNotLoaded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceBay), 1)]
        LoadUnitSourceBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitShutterOpen), 1)]
        LoadUnitShutterOpen,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitShutterInvalid), 1)]
        LoadUnitShutterInvalid,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitShutterClosed), 1)]
        LoadUnitShutterClosed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitPresentInCell), 1)]
        LoadUnitPresentInCell,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitOtherBay), 1)]
        LoadUnitOtherBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitSourceElevator), 1)]
        LoadUnitSourceElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitMissingOnElevator), 1)]
        LoadUnitMissingOnElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitMissingOnBay), 1)]
        LoadUnitMissingOnBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitUndefinedUpper), 1)]
        LoadUnitUndefinedUpper,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitUndefinedBottom), 1)]
        LoadUnitUndefinedBottom,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(FirstTestFailed), 1)]
        FirstTestFailed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(FullTestFailed), 1)]
        FullTestFailed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(WarehouseNotEmpty), 1)]
        WarehouseNotEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SensorZeroBayNotActiveAtEnd), 1)]
        SensorZeroBayNotActiveAtEnd,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityRightSensorWasTriggered), 2)]
        SecurityRightSensorWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(VerticalPositionChanged), 1)]
        VerticalPositionChanged,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidBay), 1)]
        InvalidBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidPositionBay), 1)]
        InvalidPositionBay,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ElevatorOverrunDetected), 2)]
        [ErrorCondition(typeof(IElevatorOverrunConditionEvaluator))]
        ElevatorOverrunDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ElevatorUnderrunDetected), 2)]
        [ErrorCondition(typeof(IElevatorOverrunConditionEvaluator))]
        ElevatorUnderrunDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ExternalBayEmpty), 1)]
        ExternalBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ExternalBayOccupied), 1)]
        ExternalBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(WmsError), 1)]
        WmsError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BayPositionDisabled), 1)]
        BayPositionDisabled,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MoveExtBayNotAllowed), 1)]
        MoveExtBayNotAllowed,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(StartPositioningBlocked), 1)]
        StartPositioningBlocked,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterCommandTimeout), 1)]
        InverterCommandTimeout,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceCommandTimeout), 1)]
        IoDeviceCommandTimeout,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorBaseCode), 1)]
        InverterErrorBaseCode = 1000,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorInvalidParameter), 1)]
        InverterErrorInvalidParameter = InverterErrorBaseCode + 1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorInvalidDataset), 1)]
        InverterErrorInvalidDataset = InverterErrorBaseCode + 2,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorParameterIsWriteOnly), 1)]
        InverterErrorParameterIsWriteOnly = InverterErrorBaseCode + 3,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorParameterIsReadOnly), 1)]
        InverterErrorParameterIsReadOnly = InverterErrorBaseCode + 4,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorEepromReadError), 1)]
        InverterErrorEepromReadError = InverterErrorBaseCode + 5,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorEepromWriteError), 1)]
        InverterErrorEepromWriteError = InverterErrorBaseCode + 6,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorEepromChecksumError), 1)]
        InverterErrorEepromChecksumError = InverterErrorBaseCode + 7,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorCannotWriteParameterWhileRunning), 1)]
        InverterErrorCannotWriteParameterWhileRunning = InverterErrorBaseCode + 8,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorDatasetValuesAreDifferent), 1)]
        InverterErrorDatasetValuesAreDifferent = InverterErrorBaseCode + 9,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorUnknownParameter), 1)]
        InverterErrorUnknownParameter = InverterErrorBaseCode + 11,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorSyntaxError), 1)]
        InverterErrorSyntaxError = InverterErrorBaseCode + 13,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorWrongPayloadLength), 1)]
        InverterErrorWrongPayloadLength = InverterErrorBaseCode + 14,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorNodeNotAvailable), 1)]
        InverterErrorNodeNotAvailable = InverterErrorBaseCode + 20,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorSyntaxError), 1)]
        InverterErrorSyntaxError2 = InverterErrorBaseCode + 30,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TelescopicBayError), 1)]
        TelescopicBayError,
    }
}
