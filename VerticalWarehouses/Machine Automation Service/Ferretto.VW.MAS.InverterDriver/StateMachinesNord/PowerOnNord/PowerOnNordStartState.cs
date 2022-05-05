using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOnNord
{
    internal class PowerOnNordStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IErrorsProvider errorProvider;

        private bool isAxisChanged;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PowerOnNordStartState(
            IInverterStateMachine parentStateMachine,
            Axis axisToSwitchOn,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.axisToSwitchOn = axisToSwitchOn;
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Power On Start Inverter {this.InverterStatus.SystemIndex}");
            this.startTime = DateTime.UtcNow;
            var oldAxis = this.InverterStatus.CommonControlWord.HorizontalAxis;

            this.InverterStatus.CommonControlWord.HorizontalAxis = this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value
                ? false
                : this.axisToSwitchOn == Axis.Horizontal;

            //this.InverterStatus.CommonControlWord.EnableVoltage = true;
            //this.InverterStatus.CommonControlWord.QuickStop = true;
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Power On Stop requested");

            this.ParentStateMachine.ChangeState(
                new PowerOnNordEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:PowerOnNordStartState, message={message}");
                this.ParentStateMachine.ChangeState(
                    new PowerOnNordErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogDebug($"2:message={message}:Parameter Id={message.ParameterId}");

                throw new NotImplementedException();
            }
            return true;
        }

        #endregion
    }
}
