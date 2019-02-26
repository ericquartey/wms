using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class DisabledVoltageState : IState
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

        public DisabledVoltageState(StateMachineHorizontalMoving stateMachineHorizontalMoving, IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineHorizontalMoving = stateMachineHorizontalMoving;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Disabled Voltage State";

        #endregion

        #region Methods

        private void notifyEventHandler(NotificationMessage notification)
        {
            var result = inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

            switch (notification.Status)
            {
                case MessageStatus.OperationEnd:
                    {
                        if (result == InverterDriverExitStatus.Success)
                        {
                            this.stateMachineHorizontalMoving.ChangeState(new OperationModeState(stateMachineHorizontalMoving, inverterDriver, eventAggregator));
                        }
                        break;
                    }
                case MessageStatus.OperationError:
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
