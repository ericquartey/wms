﻿using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

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

        /// <inheritdoc />
        public abstract void ProcessCommandMessage(CommandMessage message);

        /// <inheritdoc />
        public abstract void ProcessNotificationMessage(NotificationMessage message);

        #endregion
    }
}
