using Ferretto.VW.CommonUtils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces
{
    public interface IStartMessageState
    {
        #region Properties

        NotificationMessage Message { get; set; }

        #endregion
    }
}
