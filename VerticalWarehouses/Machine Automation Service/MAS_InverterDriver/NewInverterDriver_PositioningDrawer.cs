using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteVerticalPosition(int targetPosition, float vMax, float acc, float dec, float weight, short offset, bool absoluteMovement)
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationError,
                    MessageVerbosity.Info, ErrorLevel.Error));
            }
            var inverterAction = new PositioningDrawer();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.PositioningDrawer_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.PositioningDrawer_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.AbsoluteMovement = absoluteMovement;
            inverterAction.MoveAlongVerticalAxisToPoint(targetPosition, vMax, acc, dec, weight, offset);
        }

        public void ExecuteVerticalPositionStop()
        {
            if (this.inverterAction == null)
            {
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationError,
                    MessageVerbosity.Info, ErrorLevel.Error));
            }

            ((ActionBlocks.PositioningDrawer)this.inverterAction).Stop();
            ((ActionBlocks.PositioningDrawer)this.inverterAction).Terminate();

            this.inverterAction.EndEvent -= this.PositioningDrawer_ThrowEndEvent;
            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;
            this.inverterAction = null;

            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;
        }

        private void PositioningDrawer_ThrowEndEvent()
        {
            this.inverterAction.EndEvent -= this.PositioningDrawer_ThrowEndEvent;
            ((ActionBlocks.PositioningDrawer)this.inverterAction).Terminate();
            this.inverterAction = null;
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Vertical position Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationEnd,
                MessageVerbosity.Info));

        }

        private void PositioningDrawer_ThrowErrorEvent()
        {
            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
               (OperationType.Positioning, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));
        }

        #endregion
    }
}
