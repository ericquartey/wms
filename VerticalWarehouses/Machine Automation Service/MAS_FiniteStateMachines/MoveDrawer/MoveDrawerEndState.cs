using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerEndState : StateBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerEndState(
            IStateMachine parentMachine,
            IDrawerOperationMessageData drawerOperationData,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IMachineSensorsStatus machineSensorsStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentMachine, logger)
        {
            this.stopRequested = stopRequested;
            this.drawerOperationData = drawerOperationData;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.machineSensorsStatus = machineSensorsStatus;
        }

        #endregion

        #region Destructors

        ~MoveDrawerEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                null,
                "Message Description",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
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
