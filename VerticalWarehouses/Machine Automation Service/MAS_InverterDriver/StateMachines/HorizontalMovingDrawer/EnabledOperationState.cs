using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class EnabledOperationState : IState
    {
        #region Fields

        private const Byte DATASET_INDEX = 0x05; //VALUE binary = 00000101

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly ParameterID paramID = ParameterID.HOMING_MODE_PARAM;

        private readonly StateMachineHorizontalMoving stateMachineHorizontalMoving;

        private readonly Byte systemIndex = 0x00; //VALUE binary = 00000000

        private readonly Object valParam;

        #endregion

        #region Constructors

        public EnabledOperationState(StateMachineHorizontalMoving stateMachineHorizontalMoving,
            IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineHorizontalMoving = stateMachineHorizontalMoving;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public String Type => "Enabled Operation State";

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
                        this.stateMachineHorizontalMoving.ChangeState(
                            new SetNewPositionState(this.stateMachineHorizontalMoving, this.inverterDriver,
                                this.eventAggregator));
                    break;
                }
                case MessageStatus.OperationError:
                {
                    this.stateMachineHorizontalMoving.ChangeState(new ErrorState(this.stateMachineHorizontalMoving,
                        this.inverterDriver, this.eventAggregator));

                    break;
                }
            }
        }

        #endregion
    }
}
