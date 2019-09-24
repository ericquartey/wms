using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineErrors
    {
        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCompletelyLoaded))]
        CradleNotCompletelyLoaded = 100032,

        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(ConditionsNotMetForPositioning))]
        ConditionsNotMetForPositioning = 100033,
    }
}
