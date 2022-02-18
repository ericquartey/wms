using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private DateTime startTime;

        private bool stopRequested;

        #endregion

        #region Constructors

        public PositioningTableDisableOperationState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.stopRequested = stopRequested;
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"Positioning Table Disable operation. StopRequested = {this.stopRequested}");
            this.startTime = DateTime.UtcNow;
            if (this.InverterStatus is IPositioningInverterStatus currentStatus)
            {
                currentStatus.TableTravelControlWord.SequenceMode = false;
                currentStatus.TableTravelControlWord.StartMotionBlock = false;
            }

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Table Stop requested");
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
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2500)
                    {
                        this.Logger.LogError($"PositioningTableDisableOperationState timeout, inverter {this.InverterStatus.SystemIndex}");
                        this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Positioning Table Disable Operation Inverter {this.InverterStatus.SystemIndex}");
                        this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    }
                    else
                    {
                        if (this.InverterStatus is IPositioningInverterStatus currentStatus)
                        {
                            currentStatus.TableTravelControlWord.EnableOperation = false;
                        }

                        var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

                        this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                        this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                    }
                }
                else
                {
                    this.ParentStateMachine.ChangeState(new PositioningTableEndState(this.ParentStateMachine, this.InverterStatus, this.Logger, this.stopRequested));
                    returnValue = true;
                }
            }
            return returnValue;
        }

        #endregion
    }
}
