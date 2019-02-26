using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class VerticalPositioningErrorState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalPositioning parent;

        #endregion

        #region Constructors

        public VerticalPositioningErrorState(StateMachineVerticalPositioning parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string Type => "Vertical Positioning Error State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
        }

        #endregion
    }
}
