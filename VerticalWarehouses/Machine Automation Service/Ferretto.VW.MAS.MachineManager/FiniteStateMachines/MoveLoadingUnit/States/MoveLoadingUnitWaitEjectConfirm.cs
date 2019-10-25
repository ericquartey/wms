using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitWaitEjectConfirm : StateBase, IMoveLoadingUnitWaitEjectConfirm
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private LoadingUnitLocation ejectBay;

        #endregion

        #region Constructors

        public MoveLoadingUnitWaitEjectConfirm(
            IBaysProvider baysProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                this.ejectBay = messageData.Destination;
            }
        }

        protected override IState OnResume()
        {
            this.baysProvider.UnloadLoadingUnit(this.ejectBay);

            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        #endregion
    }
}
