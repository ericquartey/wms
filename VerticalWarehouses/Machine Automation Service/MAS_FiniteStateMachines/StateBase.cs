using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public abstract class StateBase : IState
    {
        #region Fields

        protected IStateMachine parentStateMachine;

        #endregion

        #region Properties

        public virtual string Type => "BaseState";

        #endregion

        #region Methods

        public abstract void ProcessCommandMessage(CommandMessage message);

        public abstract void ProcessNotificationMessage(NotificationMessage message);

        #endregion
    }
}
