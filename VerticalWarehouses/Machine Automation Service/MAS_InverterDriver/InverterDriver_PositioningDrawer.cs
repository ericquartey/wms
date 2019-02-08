using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteVerticalPosition(int targetPosition, float vMax, float acc, float dec, float weight, short offset)
        {
            if(this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.Error, "Inverter action has already defined", Verbosity.Info));
            }
            var inverterAction = new PositioningDrawer();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.PositioningDrawer_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.PositioningDrawer_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.driver;
            inverterAction.Initialize();

            inverterAction.MoveAlongVerticalAxisToPoint(targetPosition, vMax, acc, dec, weight, offset);
            
        }

        private void PositioningDrawer_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));

            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;
        }

        private void PositioningDrawer_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Vertical position Ended", Verbosity.Info));

            this.inverterAction.EndEvent -= this.PositioningDrawer_ThrowEndEvent;
        }

        #endregion Methods
    }
}
