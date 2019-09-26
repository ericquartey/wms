using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines
{
    internal class WeightAcquisitionMoveBackToStartPositionState : StateBase, IWeightAcquisitionMoveBackToStartPositionState
    {

        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public WeightAcquisitionMoveBackToStartPositionState(
            IElevatorProvider elevatorProvider,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<StateBase> logger)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            if (elevatorProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorProvider));
            }

            this.elevatorProvider = elevatorProvider;
        }

        #endregion



        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if (commandMessage.Data is WeightAcquisitionCommandMessageData messageData)
            {
                this.elevatorProvider.MoveToVerticalPosition(
                    messageData.InitialPosition,
                    DataModels.FeedRateCategory.VerticalManualMovementsAfterZero,
                    BayNumber.None);        //TODO correct bay number
            }
        }

        #endregion
    }
}
