﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    public class SwitchAxisStartState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchAxisStartState(
            Axis axisToSwitchOn,
            IoStatus status,
            IoIndex index,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs)
            {
                this.Logger.LogTrace($"1:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("2:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Data &&
                               message.ValidOutputs;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.Logger.LogTrace($"2:this.Axis to switch on={this.axisToSwitchOn}:Cradle motor on{message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("3:Change State to SwitchOnMotorState");
                    this.ParentStateMachine.ChangeState(new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.Logger, this.ParentStateMachine));
                }
            }
        }

        public override void Start()
        {
            var switchOffAxisIoMessage = new IoWriteMessage();

            this.Logger.LogTrace($"1:Switch off axis IO={switchOffAxisIoMessage}");

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOffAxisIoMessage.SwitchElevatorMotor(false);
                    break;

                case Axis.Vertical:
                    switchOffAxisIoMessage.SwitchCradleMotor(false);
                    break;
            }

            switchOffAxisIoMessage.SwitchPowerEnable(true);

            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOffAxisIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(switchOffAxisIoMessage);
        }

        #endregion
    }
}
