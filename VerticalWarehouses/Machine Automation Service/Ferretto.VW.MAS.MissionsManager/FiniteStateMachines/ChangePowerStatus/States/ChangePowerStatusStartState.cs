using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangePowerStatus.States
{
    internal class ChangePowerStatusStartState : StateBase, IChangePowerStatusStartState
    {

        #region Fields

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public ChangePowerStatusStartState(
            IBaysProvider baysProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysProvider = baysProvider;
        }

        #endregion



        #region Methods

        protected override void OnEnter(IMessageData data)
        {
            if (data is IPowerEnableMessageData messageData)
            {
                if (messageData.Enable)
                {
                }
            }
        }

        #endregion
    }
}
