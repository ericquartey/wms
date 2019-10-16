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

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyLoadedDuringPickup))]
        CradleNotCorrectlyLoadedDuringPickup = 100035,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCorrectlyUnloadedDuringDeposit))]
        CradleNotCorrectlyUnloadedDuringDeposit = 100036,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterPickup))]
        ZeroSensorErrorAfterPickup = 100037,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ZeroSensorErrorAfterDeposit))]
        ZeroSensorErrorAfterDeposit = 100038,
    }
}
