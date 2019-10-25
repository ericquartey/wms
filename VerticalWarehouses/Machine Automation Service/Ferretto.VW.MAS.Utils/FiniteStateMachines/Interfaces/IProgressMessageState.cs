using Ferretto.VW.CommonUtils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces
{
    public interface IProgressMessageState
    {
        #region Properties

        NotificationMessage Message { get; set; }

        #endregion
    }
}
