﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableEnableOperationState : InverterStateBase
    {
        #region Fields

        private const int CheckDelayTime = 100;

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
            this.startTime = DateTime.MinValue;

            this.Inverter.TableTravelControlWord.EnableOperation = true;
            this.Inverter.TableTravelControlWord.Resume = false;
            this.Inverter.TableTravelControlWord.HorizontalAxis = this.data.AxisMovement == Axis.Horizontal;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.Inverter.TableTravelControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningTableStopState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger));
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
                        var delayElapsed = DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > CheckDelayTime;
                        if (delayElapsed)
                        {
                            this.ParentStateMachine.ChangeState(new PositioningTableStartMovingState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                            returnValue = true;
                        }
                    }
                }
            }
            return returnValue;
        }

        #endregion
    }
}
