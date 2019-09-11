using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoDataLayer;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly ILogger logger;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerStateMachine(
            IEventAggregator eventAggregator,
            ISetupStatusProvider setupStatusProvider,
            IMachineSensorsStatus machineSensorsStatus,
            IGeneralInfoConfigurationDataLayer generalInfoDataLayer,
            IVerticalAxisDataLayer verticalAxis,
            IHorizontalAxisDataLayer horizontalAxis,
            IDrawerOperationMessageData drawerOperationData,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.setupStatusProvider = setupStatusProvider;
            this.logger = logger;
            this.machineSensorsStatus = machineSensorsStatus;
            this.generalInfoDataLayer = generalInfoDataLayer;
            this.verticalAxis = verticalAxis;
            this.horizontalAxis = horizontalAxis;
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
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
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

            if (this.drawerOperationData.Operation != DrawerOperation.Deposit && !this.machineSensorsStatus.IsDrawerCompletelyOffCradle)
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
                this.drawerOperationData.Step = DrawerOperationStep.None;

                this.CurrentState = new MoveDrawerStartState(
                    this,
                    this.drawerOperationData,
                    this.generalInfoDataLayer,
                    this.verticalAxis,
                    this.horizontalAxis,
                    this.machineSensorsStatus,
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
