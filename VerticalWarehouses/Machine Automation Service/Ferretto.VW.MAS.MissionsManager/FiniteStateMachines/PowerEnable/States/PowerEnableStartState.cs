using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.PowerEnable.States
{
    internal class PowerEnableStartState : StateBase, IPowerEnableStartState
    {

        #region Fields

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public PowerEnableStartState(
            IBaysProvider baysProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysProvider = baysProvider;
        }

        #endregion



        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if(commandMessage.Data is IPowerEnableMessageData messageData)
            {
                if(messageData.Enable)
                {
                }
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            return this;
        }

        #endregion
    }
}
