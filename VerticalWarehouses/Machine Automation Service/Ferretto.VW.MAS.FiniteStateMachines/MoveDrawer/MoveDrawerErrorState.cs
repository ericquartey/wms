using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    internal class MoveDrawerErrorState : StateBase
    {
        #region Fields

        private readonly Axis axis;

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly FieldNotificationMessage errorMessage;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerErrorState(
            IStateMachine parentMachine,
            FieldNotificationMessage errorMessage,
            IDrawerOperationMessageData drawerOperationData,
            Axis axis,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.errorMessage = errorMessage;
            this.drawerOperationData = drawerOperationData;
            this.axis = axis;
        }

        #endregion

        #region Destructors

        ~MoveDrawerErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process CommandMessage {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process FieldNotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterStop && message.Status == MessageStatus.OperationError)
            {
                var notificationMessage = new NotificationMessage(
                    this.drawerOperationData,
                    $"{FieldMessageType.InverterStop} Error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var stopMessage = new FieldCommandMessage(
                null,
                $"Reset Inverter Axis {this.axis}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var notificationMessage = new NotificationMessage(
                this.drawerOperationData,
                $"{MessageType.DrawerOperation} Error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                MessageStatus.OperationError);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
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
