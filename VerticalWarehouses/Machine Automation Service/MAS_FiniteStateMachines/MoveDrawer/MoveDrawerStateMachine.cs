using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly ILogger logger;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly ISetupStatus setupStatus;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerStateMachine(
            IEventAggregator eventAggregator,
            ISetupStatus setupStatus,
            IMachineSensorsStatus machineSensorsStatus,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IDrawerOperationMessageData drawerOperationData,
            ILogger logger)
            : base(eventAggregator, logger)
        {
            this.setupStatus = setupStatus;
            this.logger = logger;
            this.machineSensorsStatus = machineSensorsStatus;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.drawerOperationData = drawerOperationData;

            this.CurrentState = new EmptyState(logger);
        }

        #endregion

        #region Destructors

        ~MoveDrawerStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ChangeState(IState newState, CommandMessage message = null)
        {
            base.ChangeState(newState, message);
        }

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            //TODO get homing status from DL. Wait until DL synchronous refactoring

            //TEMP Check if homing has been done: if not, send a message of error
            var homingDone = true; // = this.setupStatus.VerticalHomingDone;
            if (!homingDone)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Vertical Homing not executed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            if (!this.machineSensorsStatus.IsSensorZeroOnCradle)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Paws are not in zero position",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            if (!this.machineSensorsStatus.IsDrawerCompletelyOffCradle)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Cradle is not completely empty",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            var bayCheckOk = false;
            switch (this.drawerOperationData.Source)
            {
                case DrawerDestination.CarouselBay1Up:
                case DrawerDestination.ExternalBay1Up:
                case DrawerDestination.InternalBay1Up:
                    bayCheckOk = this.machineSensorsStatus.IsDrawerInBay1Up;
                    break;
            }

            if (!bayCheckOk)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Drawer not found in selected bay",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            lock (this.CurrentState)
            {
                this.CurrentState = new MoveDrawerStartState(
                    this,
                    this.drawerOperationData,
                    this.dataLayerConfigurationValueManagement,
                    this.machineSensorsStatus,
                    DrawerOperationStep.None,
                    this.Logger);
                this.CurrentState?.Start();
            }
        }

        public override void Stop()
        {
            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
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
