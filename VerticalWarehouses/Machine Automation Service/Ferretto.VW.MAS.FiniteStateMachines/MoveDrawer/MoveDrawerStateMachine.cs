using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Models;
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

        private readonly IMoveDrawerMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerStateMachine(
            bool isOneKMachine,
            BayNumber requestingBay,
            ISetupStatusProvider setupStatusProvider,
            IMachineSensorsStatus machineSensorsStatus,
            IGeneralInfoConfigurationDataLayer generalInfoDataLayer,
            IVerticalAxisDataLayer verticalAxis,
            IHorizontalAxisDataLayer horizontalAxis,
            IDrawerOperationMessageData drawerOperationData,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(requestingBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.machineData = new MoveDrawerMachineData(isOneKMachine,
                setupStatusProvider,
                machineSensorsStatus,
                generalInfoDataLayer,
                verticalAxis,
                horizontalAxis,
                drawerOperationData,
                requestingBay,
                eventAggregator,
                logger,
                serviceScopeFactory);
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
        public override void Start()
        {
            var homingDone = this.machineData.SetupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Vertical Homing not executed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    this.RequestingBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            if (!this.machineData.MachineSensorsStatus.IsSensorZeroOnCradle)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Paws are not in zero position",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    this.RequestingBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            if (!this.machineData.MachineSensorsStatus.IsDrawerCompletelyOffCradle)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Cradle is not completely empty",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.DrawerOperation,
                    this.RequestingBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            var bayCheckOk = false;
            switch (this.machineData.DrawerOperationData.Source)
            {
                case DrawerDestination.CarouselBay1Up:
                case DrawerDestination.ExternalBay1Up:
                case DrawerDestination.InternalBay1Up:
                    bayCheckOk = this.machineData.MachineSensorsStatus.IsDrawerInBay1Up;
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
                    this.RequestingBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error,
                    MessageVerbosity.Error);

                this.PublishNotificationMessage(notificationMessage);
                return;
            }

            lock (this.CurrentState)
            {
                this.machineData.DrawerOperationData.Step = DrawerOperationStep.None;

                var state = new MoveDrawerStateData(this, this.machineData);
                this.CurrentState = new MoveDrawerStartState(state);
                this.CurrentState?.Start();
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
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
