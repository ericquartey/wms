using Ferretto.VW.CommonUtils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces
{
    public interface IMessageState
    {
        #region Properties

        NotificationMessage Message { get; set; }

        #endregion
    }
}
