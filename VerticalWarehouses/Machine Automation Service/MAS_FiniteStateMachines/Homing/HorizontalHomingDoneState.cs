using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The horizontal axis homing is done
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent, INewInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            if (!this.parent.HorizontalHomingAlreadyDone)
            {
                this.parent.HorizontalHomingAlreadyDone = true;

                //x this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));

                //this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Subscribe(this.notifyEventHandler);

                // execute switch vertical
                // this.remoteIO.SwitchHorizontalToVertical();
            }
            else
            {
                this.parent.ChangeState(new HomingDoneState(this.parent, this.driver, this.data, this.eventAggregator));
            }
        }

        #endregion

        #region Properties

        public string Type => "Horizontal homing done";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            //if (notification.OperationType == OperationType.Switch)
            //{
            //    switch (notification.OperationStatus)
            //    {
            //        case OperationStatus.End:
            //            {
            //                this.parent.ChangeState(new VerticalSwitchDoneState(this.parent, this.driver, this.data, this.eventAggregator));
            //                break;
            //            }
            //        case OperationStatus.Error:
            //            {
            //                break;
            //            }
            //        default:
            //            {
            //                break;
            //            }
            //    }
            //}
        }

        #endregion
    }
}
