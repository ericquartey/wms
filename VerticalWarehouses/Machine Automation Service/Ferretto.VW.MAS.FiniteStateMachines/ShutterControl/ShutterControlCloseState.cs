using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterControl
{
    public class ShutterControlCloseState : StateBase
    {

        #region Fields

        private readonly IShutterTestStatusChangedMessageData shutterControlMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterControlCloseState(
            IStateMachine parentMachine,
            IShutterTestStatusChangedMessageData shutterControlMessageData,
            ILogger logger,
            bool stopRequested = false)
            : base(parentMachine, logger)
        {
            this.shutterControlMessageData = shutterControlMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterControlCloseState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

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

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        /// <inheritdoc/>
        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        // TEMP Do it need to make something?!
                        break;

                    case MessageStatus.OperationEnd:
                        if (message.Data is InverterShutterPositioningFieldMessageData s)
                        {
                            switch (s.ShutterPosition)
                            {
                                case ShutterPosition.Half:
                                    this.ParentStateMachine.ChangeState(new ShutterControlHalfOpenState(this.ParentStateMachine, this.shutterControlMessageData, ShutterMovementDirection.Up, this.Logger));
                                    break;

                                case ShutterPosition.Opened:
                                    this.ParentStateMachine.ChangeState(new ShutterControlOpenState(this.ParentStateMachine, this.shutterControlMessageData, this.Logger));
                                    break;

                                case ShutterPosition.Closed:
                                case ShutterPosition.Intermediate:
                                    var errorMessage = new FieldNotificationMessage(
                                        null,
                                        $"Invalid position of shutter",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.ShutterPositioning,
                                        MessageStatus.OperationError,
                                        (byte)InverterIndex.Slave2);
                                    this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, errorMessage, this.Logger));
                                    break;

                                case ShutterPosition.None:
                                    break;
                            }
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, message, this.Logger));
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            //TEMP 1. Wait Delay time (ms)

            //TEMP 2. Send a command to move shutter to:
            //    - Open position, if shutter is of type Shutter2Type
            //    - Half-open position, if shutter is of type Shutter3Type
            Thread.Sleep(this.shutterControlMessageData.Delay);

            var shutterPositionTarget = ShutterPosition.None;
            switch (this.shutterControlMessageData.ShutterType)
            {
                case ShutterType.Shutter2Type:
                    shutterPositionTarget = ShutterPosition.Opened;
                    break;

                case ShutterType.Shutter3Type:
                    shutterPositionTarget = ShutterPosition.Half;
                    break;

                case ShutterType.NoType:
                    break;
            }

            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                ShutterMovementDirection.Up,
                this.shutterControlMessageData.ShutterType,
                this.shutterControlMessageData.SpeedRate);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)InverterIndex.Slave2);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterControlEndState(this.ParentStateMachine, this.shutterControlMessageData, this.Logger, true));
        }

        #endregion
    }
}
