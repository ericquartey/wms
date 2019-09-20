using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines
{
    internal class WeightAcquisitionInMotionSamplingState : StateBase, IWeightAcquisitionInMotionSamplingState
    {

        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public WeightAcquisitionInMotionSamplingState(
            IElevatorProvider elevatorProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.elevatorProvider = elevatorProvider;
        }

        #endregion



        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if(commandMessage is WeightAcquisitionCommandMessageData messageData)
            {
                this.elevatorProvider.RunInMotionCurrentSampling(messageData.Displacement, messageData.NetWeight, BayNumber.ElevatorBay);
            }
        }

        #endregion
    }
}
