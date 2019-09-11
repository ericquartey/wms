using System.Diagnostics;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private ShutterPosition currentShutterPosition;

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

        #region Destructors

        ~ShutterPositioningStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.InverterStatus is IAglInverterStatus aglStatus)
            {
                if (aglStatus.ShutterType == ShutterType.Shutter2Type && this.shutterPositionData.ShutterPosition == ShutterPosition.Half)
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
            this.InverterStatus.OperatingMode = (ushort)InverterOperationMode.ProfileVelocity;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.SetOperatingModeParam, this.InverterStatus.OperatingMode);

            this.Logger.LogTrace($"4:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

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
            var byteMessage = message.GetWriteMessage();
            this.Logger.LogTrace($"5:inverterMessage={message}");
            this.ParentStateMachine.EnqueueMessage(message);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Stop");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger, true));
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
                        var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetVelocityParam, this.shutterPositionData.SpeedRate);
                        var byteData = data.GetWriteMessage();

                        this.Logger.LogTrace($"5:inverterMessage={data}");
                        this.ParentStateMachine.EnqueueMessage(data);
                    }
                    break;

                case InverterParameterId.ShutterTargetVelocityParam:
                    {
                        var byteDataReceived = message.GetWriteMessage();
                        var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterAbsoluteEnable, (this.shutterPositionData.MovementType == MovementType.Absolute) ? 1 : 0);
                        var byteData = data.GetWriteMessage();

                        this.Logger.LogTrace($"6:inverterMessage={data}");
                        this.ParentStateMachine.EnqueueMessage(data);

                        returnValue = true;
                    }
                    break;

                case InverterParameterId.ShutterAbsoluteEnable:
                    {
                        var byteDataReceived = message.GetWriteMessage();
                        if (this.shutterPositionData.MovementType == MovementType.Relative)
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningEnableVoltageState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                        }
                        else
                        {
                            // Absolute positioning
                            // calculate revs and dataset
                            int revs = 0;
                            var dataset = InverterDataset.ActualDataset;
                            switch (this.shutterPositionData.ShutterPosition)
                            {
                                case ShutterPosition.Opened:
                                    dataset = InverterDataset.ShutterAbsoluteOpen;
                                    revs = this.shutterPositionData.HigherDistance;
                                    if (this.currentShutterPosition == ShutterPosition.Closed)
                                    {
                                        revs += this.shutterPositionData.LowerDistance;
                                    }
                                    break;

                                case ShutterPosition.Closed:
                                    dataset = InverterDataset.ShutterAbsoluteClose;
                                    revs = this.shutterPositionData.LowerDistance;
                                    if (this.currentShutterPosition == ShutterPosition.Opened)
                                    {
                                        revs += this.shutterPositionData.HigherDistance;
                                    }
                                    break;

                                case ShutterPosition.Half:
                                    dataset = InverterDataset.ShutterAbsoluteHalf;
                                    if (this.shutterPositionData.ShutterMovementDirection == ShutterMovementDirection.Down)
                                    {
                                        revs = this.shutterPositionData.HigherDistance;
                                    }
                                    else
                                    {
                                        revs = this.shutterPositionData.LowerDistance;
                                    }
                                    break;

                                default:
                                    if (Debugger.IsAttached)
                                        Debugger.Break();
                                    break;
                            }
                            var data = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ShutterAbsoluteRevs, revs, dataset);
                            var byteData = data.GetWriteMessage();

                            this.Logger.LogTrace($"7:inverterMessage={data}");
                            this.ParentStateMachine.EnqueueMessage(data);

                            returnValue = true;
                        }
                    }
                    break;

                case InverterParameterId.ShutterAbsoluteRevs:
                    {
                        var byteDataReceived = message.GetWriteMessage();
                        this.ParentStateMachine.ChangeState(new ShutterPositioningEnableVoltageState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
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

        #endregion
    }
}
