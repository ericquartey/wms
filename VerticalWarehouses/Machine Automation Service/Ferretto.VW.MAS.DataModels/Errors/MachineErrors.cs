using Ferretto.VW.MAS.DataModels.Resources;

namespace Ferretto.VW.MAS.DataModels.Errors
{
    public enum MachineErrors
    {
        [ErrorDescription(typeof(ErrorDescriptions), typeof(ErrorReasons), nameof(CradleNotCompletelyLoaded))]
        CradleNotCompletelyLoaded = 100032,
    }
}
