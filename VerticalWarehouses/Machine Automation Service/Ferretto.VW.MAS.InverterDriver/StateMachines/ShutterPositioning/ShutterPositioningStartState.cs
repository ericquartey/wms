using System.Diagnostics;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private ShutterPosition currentShutterPosition;

        private InverterDataset dataset;

        private short duration;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.shutterPositionData = shutterPositionData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Shutter positioning Start state");

            if (this.InverterStatus is IAglInverterStatus aglStatus)
            {
                if (this.shutterPositionData.ShutterType == ShutterType.TwoSensors
                    &&
                    this.shutterPositionData.ShutterPosition == ShutterPosition.Half)
                {
                    this.Logger.LogError($"2:Error unavailable position for shutter {this.InverterStatus.SystemIndex}");

                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));

                    return;
                }

                if (aglStatus.CurrentShutterPosition == this.shutterPositionData.ShutterPosition)
                {
                    this.Logger.LogWarning($"3:Warning position {this.shutterPositionData.ShutterPosition} already reached for shutter {this.InverterStatus.SystemIndex}");

                    this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));

                    return;
                }
                this.currentShutterPosition = aglStatus.CurrentShutterPosition;
            }
            this.CalculateDatasetAndDuration();
            this.InverterStatus.OperatingMode = (ushort)InverterOperationMode.ProfileVelocity;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.SetOperatingModeParam, this.InverterStatus.OperatingMode);

            this.Logger.LogTrace($"4:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var notificationMessage = new FieldNotificationMessage(
                this.shutterPositionData,
                $"Shutter Positioning Start",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.ShutterPositioning,
                MessageStatus.OperationStart,
                this.InverterStatus.SystemIndex);
            this.Logger.LogDebug("Inverter Shutter Positioning Start State Start");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            var message = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, (ushort)this.shutterPositionData.ShutterPosition);
            var byteMessage = message.ToBytes();
            this.Logger.LogTrace($"5:inverterMessage={message}");
            this.ParentStateMachine.EnqueueCommandMessage(message);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Shutter Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new ShutterPositioningStopState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.shutterPositionData,
                    this.Logger));
        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            var returnValue = false;

            //this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");
            //this.Logger.LogTrace($"2:message={message}:Parameter ID={message.ParameterId}");

            switch (message.ParameterId)
            {
                case InverterParameterId.ShutterTargetPosition:
                    {
                        var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetVelocityParam, this.shutterPositionData.SpeedRate, this.dataset);
                        var byteData = data.ToBytes();

                        this.ParentStateMachine.EnqueueCommandMessage(data);
                        this.Logger.LogDebug($"Set high velocity: {this.shutterPositionData.SpeedRate}; dataset: {(int)this.dataset}");
                    }
                    break;

                case InverterParameterId.ShutterTargetVelocityParam:
                    {
                        var byteDataReceived = message.ToBytes();
                        var speed = (this.shutterPositionData.MovementType == MovementType.Absolute) ? this.shutterPositionData.LowerSpeed : this.shutterPositionData.SpeedRate;
                        var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterLowVelocity, speed, this.dataset);
                        var byteData = data.ToBytes();

                        this.ParentStateMachine.EnqueueCommandMessage(data);
                        this.Logger.LogDebug($"Set low velocity: {speed}; dataset: {(int)this.dataset}");

                        returnValue = true;
                    }
                    break;

                case InverterParameterId.ShutterLowVelocity:
                    {
                        var byteDataReceived = message.ToBytes();
                        if (this.duration > 0)
                        {
                            var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterHighVelocityDuration, this.duration);
                            var byteData = data.ToBytes();

                            this.ParentStateMachine.EnqueueCommandMessage(data);
                            this.Logger.LogDebug($"Set duration: {this.duration}");

                            returnValue = true;
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningEnableOperationState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                        }
                    }
                    break;

                case InverterParameterId.ShutterHighVelocityDuration:
                    {
                        var byteDataReceived = message.ToBytes();
                        this.ParentStateMachine.ChangeState(new ShutterPositioningEnableOperationState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                    }
                    break;

                default:
                    break;
            }

            return returnValue;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        private void CalculateDatasetAndDuration()
        {
            this.dataset = InverterDataset.ActualDataset;
            this.duration = 0;
            if (this.shutterPositionData.MovementType == MovementType.Absolute)
            {
                switch (this.shutterPositionData.ShutterPosition)
                {
                    case ShutterPosition.Opened:
                        this.dataset = InverterDataset.ShutterAbsoluteOpen;
                        this.duration = (short)this.shutterPositionData.HighSpeedDurationOpen;
                        if (this.currentShutterPosition != ShutterPosition.Closed)
                        {
                            this.duration = (short)(this.duration * 0.45);
                        }
                        break;

                    case ShutterPosition.Closed:
                        this.dataset = InverterDataset.ShutterAbsoluteClose;
                        this.duration = (short)this.shutterPositionData.HighSpeedDurationClose;
                        if (this.currentShutterPosition != ShutterPosition.Opened)
                        {
                            this.duration = (short)(this.duration * 0.45);
                        }
                        break;

                    case ShutterPosition.Half:
                        this.dataset = InverterDataset.ShutterAbsoluteHalf;
                        if (this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Down)
                        {
                            this.duration = (short)this.shutterPositionData.HighSpeedDurationOpen;
                        }
                        else
                        {
                            this.duration = (short)this.shutterPositionData.HighSpeedDurationClose;
                        }
                        this.duration = (short)(this.duration * 0.45);
                        break;

                    default:
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                        break;
                }
            }
        }

        #endregion
    }
}
