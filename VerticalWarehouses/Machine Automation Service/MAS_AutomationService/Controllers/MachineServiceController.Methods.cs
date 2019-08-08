using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public partial class MachineServiceController
    {
        #region Fields

        private const decimal ChainLength = 2850.0M;

        #endregion

        #region Methods

        private void ExecuteSearchHorizontalZero_Method()
        {
            var actualSpeed = this.horizontalAxis.MaxEmptySpeedHA * this.horizontalManualMovements.FeedRateHM;

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.FindZero,
                ChainLength,
                actualSpeed,
                this.horizontalAxis.MaxEmptyAccelerationHA,
                this.horizontalAxis.MaxEmptyDecelerationHA,
                0,
                0,
                0);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    messageData,
                    $"Execute Find Horizontal Zero Positioning Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Positioning));
        }

        #endregion
    }
}
