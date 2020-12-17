using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableEnableOperationState : InverterStateBase
    {
        #region Fields

        private const int CheckDelayTime = 200;

        private readonly IInverterPositioningFieldMessageData data;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PositioningTableEnableOperationState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;

            this.Inverter = inverterStatus;
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug("Inverter Enable Operation");
            this.startTime = DateTime.UtcNow;

            this.Inverter.TableTravelControlWord.HorizontalAxis = this.data.AxisMovement == Axis.Horizontal;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.Inverter.TableTravelControlWord.Value));

            this.Inverter.TableTravelControlWord.EnableOperation = true;
            this.Inverter.TableTravelControlWord.Resume = false;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.Inverter.TableTravelControlWord.Value));

            this.startTime = DateTime.MinValue;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningTableDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger,
                    true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

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
                // we must wait at least 100ms between EnableOperation and start moving
                if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    if (this.startTime == DateTime.MinValue)
                    {
                        this.startTime = DateTime.UtcNow;
                    }
                    else
                    {
                        if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > CheckDelayTime)
                        {
                            this.ParentStateMachine.ChangeState(
                                new PositioningTableWaitState(
                                    this.ParentStateMachine,
                                    this.data,
                                    this.InverterStatus as IPositioningInverterStatus,
                                    this.Logger));
                            returnValue = true;
                        }
                    }
                }
                else if (this.startTime != DateTime.MinValue
                    && DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000
                    )
                {
                    this.Logger.LogError($"PositioningTableEnableOperation position timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }
            return returnValue;
        }

        #endregion
    }
}
