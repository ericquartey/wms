using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineErrors
    {
        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCompletelyLoaded))]
        CradleNotCompletelyLoaded = 100032,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForPositioning))]
        ConditionsNotMetForPositioning = 100033,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForRunning))]
        ConditionsNotMetForRunning = 100034,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(SecurityWasTriggered))]
        SecurityWasTriggered = 100035,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterFaultStateDetected))]
        InverterFaultStateDetected = 100036,

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

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorUnknown), 1)]
        InverterErrorUnknown = InverterErrorBaseCode + 15,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorNodeNotAvailable), 1)]
        InverterErrorNodeNotAvailable = InverterErrorBaseCode + 20,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(InverterErrorSyntaxError), 1)]
        InverterErrorSyntaxError2 = InverterErrorBaseCode + 30,
    }
}
