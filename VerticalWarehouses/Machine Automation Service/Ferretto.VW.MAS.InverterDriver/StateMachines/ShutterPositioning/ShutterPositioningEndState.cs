using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningEndState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private readonly bool stopRequested;

        #endregion

        #region Constructors

        public ShutterPositioningEndState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.shutterPositionData = shutterPositionData;
            this.stopRequested = stopRequested;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Notify Positioning End. StopRequested = {this.stopRequested}");

            if (this.InverterStatus is AglInverterStatus currentStatus)
            {
                this.shutterPositionData.ShutterPosition = currentStatus.CurrentShutterPosition;
            }
            var endNotification = new FieldNotificationMessage(
                this.shutterPositionData,
                "Shutter Positioning complete",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.InverterDriver,
                FieldMessageType.ShutterPositioning,
                (this.stopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
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
