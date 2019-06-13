using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterControl
{
    public class ShutterControlErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private readonly IShutterControlMessageData shutterControlMessageData;

        private bool disposed;

        private FieldCommandMessage stopMessage;

        #endregion

        #region Constructors

        public ShutterControlErrorState(IStateMachine parentMachine, IShutterControlMessageData shutterControlMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.errorMessage = errorMessage;
            this.shutterControlMessageData = shutterControlMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterControlErrorState()
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

        /// <inheritdoc/>
        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterStop)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        //TEMP Do nothing
                        break;

                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationError:
                        var notificationMessageData = new ShutterControlMessageData(
                            this.shutterControlMessageData.BayNumber,
                            this.shutterControlMessageData.Delay,
                            this.shutterControlMessageData.NumberCycles,
                            this.shutterControlMessageData.SpeedRate);
                        notificationMessageData.Verbosity = MessageVerbosity.Error;
                        notificationMessageData.ExecutedCycles = this.shutterControlMessageData.ExecutedCycles;

                        var notificationMessage = new NotificationMessage(
                            notificationMessageData,
                            "Shutter Control Test stopped due to an error",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.Positioning,
                            MessageStatus.OperationError,
                            ErrorLevel.Error);

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            //TEMP The FSM must be defined the inverter to stop (by the inverter index)
            var stopMessageData = new InverterStopFieldMessageData(InverterIndex.Slave2);
            this.stopMessage = new FieldCommandMessage(
                stopMessageData,
                "Reset ShutterPositioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning);

            this.logger.LogTrace($"1:Publish Field Command Message processed: {this.stopMessage.Type}, {this.stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.stopMessage);
        }

        /// <inheritdoc/>
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
