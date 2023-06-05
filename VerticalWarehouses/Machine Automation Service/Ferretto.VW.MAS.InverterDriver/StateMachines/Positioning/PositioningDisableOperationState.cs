using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private readonly int inverterResponseTimeout;

        private readonly IMachineProvider machineProvider;

        private DateTime startTime;

        private bool stopRequested;

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
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.machineProvider = this.ParentStateMachine.GetRequiredService<IMachineProvider>();

            this.inverterResponseTimeout = this.machineProvider.GetInverterResponseTimeout();
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
            this.startTime = DateTime.UtcNow;
            this.Inverter.PositionControlWord.NewSetPoint = false;
            this.Inverter.PositionControlWord.ImmediateChangeSet = false;
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
            this.Logger.LogDebug("1:Positioning Stop requested");
            this.stopRequested = true;
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
                if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.inverterResponseTimeout)
                    {
                        this.Logger.LogError($"PositioningDisableOperationState timeout, inverter {this.InverterStatus.SystemIndex}");
                        this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Positioning Disable Operation Inverter {this.InverterStatus.SystemIndex}");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    }
                    else
                    {
                        this.Inverter.PositionControlWord.EnableOperation = false;
                        this.ParentStateMachine.EnqueueCommandMessage(
                            new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.ControlWord,
                                this.InverterStatus.CommonControlWord.Value));
                    }
                }
                else
                {
                    this.ParentStateMachine.ChangeState(
                        new PositioningEndState(
                            this.ParentStateMachine,
                            this.Inverter,
                            this.Logger,
                            this.stopRequested));

                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
