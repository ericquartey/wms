using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IState
    {


        #region Methods

        IState CommandReceived(CommandMessage commandMessage);

        void Enter(CommandMessage commandMessage);

        void Exit();

        IState NotificationReceived(NotificationMessage notificationMessage);

        #endregion
    }
}
