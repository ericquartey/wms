using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private readonly ShutterPosition shutterPosition;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        private FieldCommandMessage stopMessage;

        #endregion

        #region Constructors

        public ShutterPositioningErrorState(IStateMachine parentMachine, IShutterPositioningMessageData shutterPositioningMessageData, ShutterPosition shutterPosition, FieldNotificationMessage errorMessage, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.shutterPosition = shutterPosition;
            this.errorMessage = errorMessage;
            this.shutterPositioningMessageData = shutterPositioningMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterPowerOff && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningMessageData);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Shuter Positioning Stopped for an error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.ShutterPositioning,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            if (this.shutterPositioningMessageData.BayNumber == 0)
            {
                var stopMessageData = new InverterStopFieldMessageData(InverterIndex.Slave2);
                this.stopMessage = new FieldCommandMessage(stopMessageData,
                 "Reset ShutterPositioning",
                 FieldMessageActor.InverterDriver,
                 FieldMessageActor.FiniteStateMachines,
                 FieldMessageType.InverterStop);
            }
            else
            {
                this.stopMessage = new FieldCommandMessage(null,
                "Reset Inverter ShutterPositioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop);
            }

            this.logger.LogTrace($"1:Publish Field Command Message processed: {this.stopMessage.Type}, {this.stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.stopMessage);
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
