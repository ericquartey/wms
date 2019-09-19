using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class SensorsProvider : BaseProvider, ISensorsProvider
    {
        #region Constructors

        public SensorsProvider(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        public bool[] GetAll()
        {
            void publishAction()
            {
                this.PublishCommand(
                    null,
                    "Sensors changed Command",
                    MessageActor.FiniteStateMachines,
                    MessageType.SensorsChanged);
            }

            var messageData = this.WaitForResponseEventAsync<SensorsChangedMessageData>(
                MessageType.SensorsChanged,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return messageData.SensorsStates;
        }

        #endregion
    }
}
