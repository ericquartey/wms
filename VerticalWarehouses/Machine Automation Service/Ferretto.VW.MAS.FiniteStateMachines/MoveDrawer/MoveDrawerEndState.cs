using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    internal class MoveDrawerEndState : StateBase
    {
        #region Fields

        private readonly IMoveDrawerMachineData machineData;

        private readonly IMoveDrawerStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerEndState(IMoveDrawerStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IMoveDrawerMachineData;
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
            this.Logger.LogTrace($"1:Process CommandMessage {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process FieldNotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            // Send a notification message about the completion of operation
            var notificationMessage = new NotificationMessage(
                this.machineData.DrawerOperationData,
                $"{MessageType.DrawerOperation} end",
                MessageActor.FiniteStateMachines,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
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
