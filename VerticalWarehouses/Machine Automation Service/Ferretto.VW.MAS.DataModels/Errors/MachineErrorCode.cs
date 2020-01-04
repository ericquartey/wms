using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineErrorCode
    {
        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(NoError))]
        NoError = -1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCompletelyLoaded))]
        CradleNotCompletelyLoaded = 1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForPositioning))]
        [ErrorCondition(typeof(IElevatorHorizontalChainZeroConditionEvaluator))]
        ConditionsNotMetForPositioning,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForRunning))]
        [ErrorCondition(typeof(ISecurityIsClearedConditionEvaluator))]
        ConditionsNotMetForRunning,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityWasTriggered))]
        [ErrorCondition(typeof(ISecurityIsClearedConditionEvaluator))]
        SecurityWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityButtonWasTriggered))]
        SecurityButtonWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityBarrierWasTriggered))]
        SecurityBarrierWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecuritySensorWasTriggered))]
        SecuritySensorWasTriggered,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterFaultStateDetected))]
        InverterFaultStateDetected,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyLoadedDuringPickup))]
        CradleNotCorrectlyLoadedDuringPickup,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyUnloadedDuringDeposit))]
        CradleNotCorrectlyUnloadedDuringDeposit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterPickup))]
        ZeroSensorErrorAfterPickup,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterDeposit))]
        ZeroSensorErrorAfterDeposit,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InvalidPresenceSensors))]
        InvalidPresenceSensors,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MissingZeroSensorWithEmptyElevator))]
        MissingZeroSensorWithEmptyElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorActiveWithFullElevator))]
        ZeroSensorActiveWithFullElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadUnitPresentOnEmptyElevator))]
        LoadUnitPresentOnEmptyElevator,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TopLevelBayOccupied))]
        TopLevelBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BottomLevelBayOccupied))]
        BottomLevelBayOccupied,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(TopLevelBayEmpty))]
        TopLevelBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(BottomLevelBayEmpty))]
        BottomLevelBayEmpty,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SensorZeroBayNotActiveAtStart))]
        SensorZeroBayNotActiveAtStart,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterConnectionError), 0)]
        InverterConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(IoDeviceConnectionError), 0)]
        IoDeviceConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LaserConnectionError), 0)]
        LaserConnectionError,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadingUnitWeightExceeded), 1)]
        LoadingUnitWeightExceeded,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(LoadingUnitWeightTooLow), 1)]
        LoadingUnitWeightTooLow,

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

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorBaseCode), 1)]
        InverterErrorBaseCode = 200000,

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

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorBaseCode), 1)]
        MachineManagerErrorBaseCode = 300000,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorNoLoadingUnitInSource), 1)]
        MachineManagerErrorNoLoadingUnitInSource = MachineManagerErrorBaseCode + 1,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitSourceDb), 1)]
        MachineManagerErrorLoadingUnitSourceDb = MachineManagerErrorBaseCode + 2,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitDestinationCell), 1)]
        MachineManagerErrorLoadingUnitDestinationCell = MachineManagerErrorBaseCode + 3,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitElevator), 1)]
        MachineManagerErrorLoadingUnitElevator = MachineManagerErrorBaseCode + 4,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitNotRemoved), 1)]
        MachineManagerErrorLoadingUnitNotRemoved = MachineManagerErrorBaseCode + 5,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitDestinationBay), 1)]
        MachineManagerErrorLoadingUnitDestinationBay = MachineManagerErrorBaseCode + 6,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitSourceCell), 1)]
        MachineManagerErrorLoadingUnitSourceCell = MachineManagerErrorBaseCode + 7,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitNotFound), 1)]
        MachineManagerErrorLoadingUnitNotFound = MachineManagerErrorBaseCode + 8,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitNotLoaded), 1)]
        MachineManagerErrorLoadingUnitNotLoaded = MachineManagerErrorBaseCode + 9,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitSourceBay), 1)]
        MachineManagerErrorLoadingUnitSourceBay = MachineManagerErrorBaseCode + 10,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitShutterOpen), 1)]
        MachineManagerErrorLoadingUnitShutterOpen = MachineManagerErrorBaseCode + 11,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitShutterClosed), 1)]
        MachineManagerErrorLoadingUnitShutterClosed = MachineManagerErrorBaseCode + 12,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitPresentInCell), 1)]
        MachineManagerErrorLoadingUnitPresentInCell = MachineManagerErrorBaseCode + 13,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitOtherBay), 1)]
        MachineManagerErrorLoadingUnitOtherBay = MachineManagerErrorBaseCode + 14,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitSourceElevator), 1)]
        MachineManagerErrorLoadingUnitSourceElevator = MachineManagerErrorBaseCode + 15,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitMissingOnElevator), 1)]
        MachineManagerErrorLoadingUnitMissingOnElevator = MachineManagerErrorBaseCode + 16,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitMissingOnBay1), 1)]
        MachineManagerErrorLoadingUnitMissingOnBay1 = MachineManagerErrorBaseCode + 17,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitMissingOnBay2), 1)]
        MachineManagerErrorLoadingUnitMissingOnBay2 = MachineManagerErrorBaseCode + 18,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(MachineManagerErrorLoadingUnitMissingOnBay3), 1)]
        MachineManagerErrorLoadingUnitMissingOnBay3 = MachineManagerErrorBaseCode + 19,
    }
}
