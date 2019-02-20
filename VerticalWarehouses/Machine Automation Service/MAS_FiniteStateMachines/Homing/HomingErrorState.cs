using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingErrorState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HomingErrorState(StateMachineHoming parent, INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.Error, "Homing Error", Verbosity.Info);
            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);
        }

        #endregion

        #region Properties

        public string Type => "Homing Error State";

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
