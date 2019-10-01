using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IEndState
    {
        #region Properties

        NotificationMessage EndMessage { get; set; }

        NotificationMessage ErrorMessage { get; set; }

        bool IsCompleted { get; set; }

        StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
