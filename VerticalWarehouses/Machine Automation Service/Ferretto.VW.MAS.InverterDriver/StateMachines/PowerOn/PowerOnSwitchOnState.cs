﻿using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn
{
    internal class PowerOnSwitchOnState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private readonly int inverterResponseTimeout;

        private readonly IMachineProvider machineProvider;

        private double minTimeout;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PowerOnSwitchOnState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.machineProvider = this.ParentStateMachine.GetRequiredService<IMachineProvider>();

            this.inverterResponseTimeout = this.machineProvider.GetInverterResponseTimeout();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Power On Switch on Inverter {this.InverterStatus.SystemIndex}");

            if (this.InverterStatus.SystemIndex == 0
                && !this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value
                && this.ParentStateMachine.GetRequiredService<IMachineProvider>().IsAxisChanged()
                )
            {
                // the first time we switch on the inverters we reset the ack of axis changed parameter
                ushort val = 10000;
                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.AxisChanged, val, InverterDataset.AxisChangeDatasetWrite);

                this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
            }

            this.minTimeout = 300;

            {
                // switch on axis
                this.InverterStatus.CommonControlWord.SwitchOn = true;

                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
            }

            this.startTime = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Power On Stop requested");

            this.ParentStateMachine.ChangeState(
                new PowerOnEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus.CommonStatusWord.IsSwitchedOn
                    && DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.minTimeout
                    )
                {
                    this.ParentStateMachine.ChangeState(new PowerOnEndState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.inverterResponseTimeout)
                {
                    this.Logger.LogError($"2:PowerOnSwitchOnState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Switch On Inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(
                        new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }

            return true;
        }

        #endregion
    }
}
