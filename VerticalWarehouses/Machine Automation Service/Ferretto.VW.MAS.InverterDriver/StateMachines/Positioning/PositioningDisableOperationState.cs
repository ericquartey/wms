﻿using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly bool stopRequested;

        #endregion

        #region Constructors

        public PositioningDisableOperationState(
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Inverter = inverterStatus;
            this.stopRequested = stopRequested;
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"Positioning Disable Operation. StopRequested = {this.stopRequested}");
            this.Inverter.PositionControlWord.EnableOperation = false;
            this.Inverter.PositionControlWord.NewSetPoint = false;
            this.Inverter.PositionControlWord.RelativeMovement = false;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.InverterStatus.CommonControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            if (this.stopRequested)
            {
                this.Logger.LogTrace("1:Stop process already active");
            }
            else
            {
                this.Logger.LogDebug("1:Positioning Stop requested");

                this.ParentStateMachine.ChangeState(
                    new PositioningStopState(
                        this.ParentStateMachine,
                        this.Inverter,
                        this.Logger));
            }
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    if (this.stopRequested)
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningQuickStopState(
                                this.ParentStateMachine,
                                this.Inverter,
                                this.Logger));
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningEndState(
                                this.ParentStateMachine,
                                this.Inverter,
                                this.Logger));
                    }

                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
