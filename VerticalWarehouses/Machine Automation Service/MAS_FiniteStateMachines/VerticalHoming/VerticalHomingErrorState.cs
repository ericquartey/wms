using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class VerticalHomingErrorState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming parent;

        #endregion

        #region Constructors

        public VerticalHomingErrorState(StateMachineVerticalHoming parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Error State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
        }

        public void NotifyMessage(Event_Message message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
        }

        #endregion
    }
}
