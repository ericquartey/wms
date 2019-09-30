using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IState
    {


        #region Properties

        NotificationMessage ErrorMessage { get; set; }

        StopRequestReason StopRequestReason { get; set; }

        #endregion



        #region Methods

        IState CommandReceived(CommandMessage commandMessage);

        void Enter(CommandMessage commandMessage);

        void Exit();

        IState NotificationReceived(NotificationMessage notificationMessage);

        IState Stop(StopRequestReason reason);

        #endregion
    }
}
