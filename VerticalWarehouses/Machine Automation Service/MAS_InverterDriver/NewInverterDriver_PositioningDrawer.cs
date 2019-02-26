using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteVerticalPosition(Int32 targetPosition, Single vMax, Single acc, Single dec, Single weight,
            Int16 offset, Boolean absoluteMovement)
        {
            if (this.inverterAction != null)
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver,
                    MessageType.Homing, MessageStatus.OperationError,
                    ErrorLevel.Error));
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
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                    MessageStatus.OperationError,
                    ErrorLevel.Error));

            ( (PositioningDrawer) this.inverterAction ).Stop();
            ( (PositioningDrawer) this.inverterAction ).Terminate();

            this.inverterAction.EndEvent -= this.PositioningDrawer_ThrowEndEvent;
            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;
            this.inverterAction = null;

            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;
        }

        private void PositioningDrawer_ThrowEndEvent()
        {
            this.inverterAction.EndEvent -= this.PositioningDrawer_ThrowEndEvent;
            ( (PositioningDrawer) this.inverterAction ).Terminate();
            this.inverterAction = null;
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Vertical position Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationEnd));
        }

        private void PositioningDrawer_ThrowErrorEvent()
        {
            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver,
                MessageType.Positioning, MessageStatus.OperationError, ErrorLevel.Error));
        }

        #endregion
    }
}
