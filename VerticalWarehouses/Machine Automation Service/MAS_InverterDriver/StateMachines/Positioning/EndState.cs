using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    internal class EndState : InverterStateBase
    {
        #region Fields

        private const int SEND_DELAY = 50;

        private const ushort STATUS_WORD_VALUE = 0x0250;

        private readonly IPositioningFieldMessageData data;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(IInverterStateMachine parentStateMachine, IPositioningFieldMessageData data, ILogger logger, bool stopRequested = false)
        {
            this.logger = logger;
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.data = data;

            if (stopRequested)
            {
                var stopParameterValue = 0x0000;

                switch (this.data.AxisMovement)
                {
                    case Axis.Horizontal:
                        stopParameterValue = 0x8000;
                        break;

                    case Axis.Vertical:
                        stopParameterValue = 0x0000;
                        break;
                }

                var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, (ushort)stopParameterValue, SEND_DELAY);

                this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueMessage(inverterMessage);
            }
            else
            {
                var messageData = new PositioningFieldMessageData(
                    this.data.AxisMovement,
                    this.data.MovementType,
                    this.data.TargetPosition,
                    this.data.TargetSpeed,
                    this.data.TargetAcceleration,
                    this.data.TargetDeceleration);
                var endNotification = new FieldNotificationMessage(messageData,
                    "Axis calibration complete",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.Positioning,
                    MessageStatus.OperationEnd);

                this.logger.LogTrace(
                    $"3:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
            this.logger.LogDebug("4:Method End");
        }

        #endregion

        #region Destructors

        ~EndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            bool returnValue = false;

            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Process Inverter Message: {message}: Axis to position={this.data.AxisMovement}");

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.data, this.logger));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload:X}:RESET_STATUS_WORD_VALUE={STATUS_WORD_VALUE:X}");

                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
                {
                    var messageData = new PositioningFieldMessageData(
                    this.data.AxisMovement,
                    this.data.MovementType,
                    this.data.TargetPosition,
                    this.data.TargetSpeed,
                    this.data.TargetAcceleration,
                    this.data.TargetDeceleration);
                    var endNotification = new FieldNotificationMessage(messageData,
                        $"{this.data.AxisMovement} positioning complete",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.Positioning,
                        MessageStatus.OperationStop);

                    this.logger.LogTrace(
                        $"4:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                    this.ParentStateMachine.PublishNotificationEvent(endNotification);

                    returnValue = true;
                }
            }

            this.logger.LogDebug("5:Method End");

            return returnValue;
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogDebug("2:Method End");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
