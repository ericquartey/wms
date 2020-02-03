using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IState
    {
        #region Methods

        IState Abort();

        IState CommandReceived(CommandMessage commandMessage);

        void Enter(CommandMessage commandMessage, IServiceProvider serviceProvider, IFiniteStateMachineData stateData);

        void Exit();

        IState NotificationReceived(NotificationMessage notificationMessage);

        IState Pause();

        IState Resume(CommandMessage commandMessage);

        IState Stop(StopRequestReason reason);

        #endregion
    }
}
