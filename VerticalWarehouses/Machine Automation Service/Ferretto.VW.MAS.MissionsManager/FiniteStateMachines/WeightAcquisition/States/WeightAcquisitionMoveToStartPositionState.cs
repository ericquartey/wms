using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines
{
    internal class WeightAcquisitionMoveToStartPositionState : StateBase, IWeightAcquisitionMoveToStartPositionState
    {

        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public WeightAcquisitionMoveToStartPositionState(
            ILogger<StateBase> logger,
            IElevatorProvider elevatorProvider)
            : base(logger)
        {
            if(elevatorProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorProvider));
            }

            this.elevatorProvider = elevatorProvider;
        }

        #endregion



        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if(commandMessage is WeightAcquisitionCommandMessageData messageData)
            {
                this.elevatorProvider.MoveToVerticalPosition(
                    messageData.InitialPosition,
                    DataModels.FeedRateCategory.VerticalManualMovementsAfterZero,
                    BayNumber.None);    //TODO - correct bay number
            }
        }

        #endregion
    }
}
