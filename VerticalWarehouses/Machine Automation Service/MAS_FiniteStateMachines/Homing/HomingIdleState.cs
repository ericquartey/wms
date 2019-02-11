using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingIdleState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HomingIdleState(StateMachineHoming parent, INewInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            //x this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));

            //this.eventAggregator.GetEvent<RemoteIO_NotificationEvent>().Subscribe(this.notifyEventHandler);

            // execute switch horizontal
        }

        #endregion

        #region Properties

        public string Type => "Homing Idle State";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {
        }

        #endregion
    }
}
