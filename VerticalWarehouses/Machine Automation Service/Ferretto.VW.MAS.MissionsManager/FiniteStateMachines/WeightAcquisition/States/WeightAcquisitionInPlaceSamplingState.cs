using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines
{
    internal class WeightAcquisitionInPlaceSamplingState : StateBase, IWeightAcquisitionInPlaceSamplingState
    {

        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public WeightAcquisitionInPlaceSamplingState(
            IElevatorProvider elevatorProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.elevatorProvider = elevatorProvider;
        }

        #endregion



        #region Methods

        protected override void OnEnter(IMessageData data)
        {
            if(data is WeightAcquisitionCommandMessageData messageData)
            {
                this.elevatorProvider.RunInPlaceCurrentSampling(messageData.InPlaceSamplingDuration, messageData.NetWeight, BayNumber.ElevatorBay);
            }
        }

        #endregion
    }
}
