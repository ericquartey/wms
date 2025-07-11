﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningErrorState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        #endregion

        #region Constructors

        public ShutterPositioningErrorState(
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
            this.Logger.LogError($"Shutter Positioning Error state");

            if (this.InverterStatus is AglInverterStatus currentStatus)
            {
                this.shutterPositionData.ShutterPosition = currentStatus.CurrentShutterPosition;
            }
            var errorNotification = new FieldNotificationMessage(
                this.shutterPositionData,
                "Inverter operation error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.ShutterPositioning,
                MessageStatus.OperationError,
                this.InverterStatus.SystemIndex,
                ErrorLevel.Error);

            this.Logger.LogTrace($"1:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(errorNotification);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            // do nothing
        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            // do nothing
            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            // do nothing
            return true;
        }

        #endregion
    }
}
