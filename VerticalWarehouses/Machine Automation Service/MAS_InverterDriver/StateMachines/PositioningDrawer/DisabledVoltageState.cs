using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.PositioningDrawer
{
    public class DisabledVoltageState : IState
    {
        #region Fields

        private const byte DATASET_INDEX = 0x05;

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private readonly StateMachinePositioningDrawer stateMachinePositioningDrawer;

        private readonly byte systemIndex = 0x00;

        private readonly object valParam;

        #endregion

        #region Constructors

        public DisabledVoltageState(StateMachinePositioningDrawer stateMachinePositioningDrawer,
            IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachinePositioningDrawer = stateMachinePositioningDrawer;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Disabled Voltage State";

        #endregion

        #region Methods

        private void notifyEventHandler(NotificationMessage notification)
        {
            var result =
                this.inverterDriver.SettingRequest(this.paramID, this.systemIndex, DATASET_INDEX, this.valParam);

            switch (notification.Status)
            {
                case MessageStatus.OperationEnd:
                    {
                        if (result == InverterDriverExitStatus.Success)
                            this.stateMachinePositioningDrawer.ChangeState(
                                new OperationModeState(this.stateMachinePositioningDrawer, this.inverterDriver,
                                    this.eventAggregator));
                        break;
                    }
                case MessageStatus.OperationError:
                    {
                        this.stateMachinePositioningDrawer.ChangeState(new ErrorState(this.stateMachinePositioningDrawer,
                            this.inverterDriver, this.eventAggregator));

                        break;
                    }
            }
        }

        #endregion
    }
}
