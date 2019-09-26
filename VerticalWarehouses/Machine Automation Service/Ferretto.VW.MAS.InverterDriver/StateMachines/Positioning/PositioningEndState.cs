﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningEndState : InverterStateBase
    {
        #region Fields

        private readonly bool stopRequested;

        #endregion

        #region Constructors

        public PositioningEndState(
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.stopRequested = stopRequested;
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
            this.Logger.LogDebug($"Notify Positioning End. StopRequested = {this.stopRequested}");

            if (this.stopRequested)
            {
                this.ParentStateMachine.PublishNotificationEvent(
                    new FieldNotificationMessage(
                        null,
                        "Message",
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStop,
                        MessageStatus.OperationEnd,
                        this.InverterStatus.SystemIndex));
            }

            this.ParentStateMachine.PublishNotificationEvent(
                new FieldNotificationMessage(
                    null,
                    "Message",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.Positioning,
                    (this.stopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd,
                    this.InverterStatus.SystemIndex));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
