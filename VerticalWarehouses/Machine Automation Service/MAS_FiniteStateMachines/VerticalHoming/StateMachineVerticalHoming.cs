using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class StateMachineVerticalHoming : IState, IStateMachine
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly IInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineVerticalHoming(IInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            //this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => this.state.Type;

        #endregion

        #region Methods

        public void ChangeState(IState newState)
        {
            this.state = newState;
        }

        public void ExecuteOperation(IdOperation code)
        {
        }

        public void Start()
        {
            // TODO check the sensors before to set the initial state
            this.state = new VerticalHomingIdleState(this, this.driver, this.data, this.eventAggregator);
        }

        #endregion

        /*
        private void notifyEventHandler(InverterDriver_Notification notification)
        {
            // do something
        }
        */
    }
}
