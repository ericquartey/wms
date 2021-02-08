using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff
{
    internal class SwitchOffWaitState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private DateTime startTime;

        #endregion

        #region Constructors

        public SwitchOffWaitState(
                    IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogTrace($"1:SwitchOffWaitState Start");
            this.startTime = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Switch Off Stop requested");

            this.ParentStateMachine.ChangeState(
                new SwitchOffEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:SwitchOffWaitState message={message}");
                this.errorProvider.RecordNew(MachineErrorCode.InverterErrorBaseCode);
                this.ParentStateMachine.ChangeState(new SwitchOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 500)
                {
                    this.ParentStateMachine.ChangeState(new SwitchOffEndState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
            }
            return returnValue;
        }

        #endregion
    }
}
