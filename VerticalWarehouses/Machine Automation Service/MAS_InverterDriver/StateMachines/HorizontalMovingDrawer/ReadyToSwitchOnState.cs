using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class ReadyToSwitchOnState : IState
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly StateMachineHorizontalMoving stateMachineHorizontalMoving;

        private readonly ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private const byte DATASET_INDEX = 0x05;  //VALUE binary = 00000101

        private readonly byte systemIndex = 0x00; //VALUE binary = 00000000

        private readonly object valParam;

        #endregion

        #region Constructors

        public ReadyToSwitchOnState(StateMachineHorizontalMoving stateMachineHorizontalMoving, IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineHorizontalMoving = stateMachineHorizontalMoving;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Ready to Switch On State";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            var result = inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

            switch (notification.OperationStatus)
            {
                case OperationStatus.End:
                    {
                        if (result == InverterDriverExitStatus.Success)
                        {
                            this.stateMachineHorizontalMoving.ChangeState(new SwitchOnState(stateMachineHorizontalMoving, inverterDriver, eventAggregator));
                        }
                        break;
                    }
                case OperationStatus.Error:
                    {
                        this.stateMachineHorizontalMoving.ChangeState(new ErrorState(stateMachineHorizontalMoving, inverterDriver, eventAggregator));

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion
    }
}
