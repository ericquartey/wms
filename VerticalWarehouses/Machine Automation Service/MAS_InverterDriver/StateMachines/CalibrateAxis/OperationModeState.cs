using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class OperationModeState : IState
    {
        #region Fields

        private const byte DATASET_INDEX = 0x05;

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private readonly StateMachineCalibrateAxis stateMachineCalibrateAxis;

        private readonly byte systemIndex = 0x00;

        private readonly object valParam;

        #endregion

        #region Constructors

        public OperationModeState(StateMachineCalibrateAxis stateMachineCalibrateAxis, IInverterDriver inverterDriver,
            IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineCalibrateAxis = stateMachineCalibrateAxis;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Operation Mode State";

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
                        this.stateMachineCalibrateAxis.ChangeState(new ShutDownState(this.stateMachineCalibrateAxis,
                            this.inverterDriver, this.eventAggregator));
                    break;
                }
                case MessageStatus.OperationError:
                {
                    this.stateMachineCalibrateAxis.ChangeState(new ErrorState(this.stateMachineCalibrateAxis,
                        this.inverterDriver, this.eventAggregator));

                    break;
                }
            }
        }

        #endregion
    }
}
