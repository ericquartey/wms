using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class ShutDownState : IState
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly StateMachineCalibrateAxis stateMachineCalibrateAxis;

        private readonly ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private const byte DATASET_INDEX = 0x05;

        private readonly byte systemIndex = 0x00;

        private readonly object valParam;

        #endregion

        #region Constructors

        public ShutDownState(StateMachineCalibrateAxis stateMachineCalibrateAxis, IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineCalibrateAxis = stateMachineCalibrateAxis;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Shut Down State";

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
                            this.stateMachineCalibrateAxis.ChangeState(new SwitchOnState(stateMachineCalibrateAxis, inverterDriver, eventAggregator));
                        }
                        break;
                    }
                case OperationStatus.Error:
                    {
                        this.stateMachineCalibrateAxis.ChangeState(new ErrorState(stateMachineCalibrateAxis, inverterDriver, eventAggregator));

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
