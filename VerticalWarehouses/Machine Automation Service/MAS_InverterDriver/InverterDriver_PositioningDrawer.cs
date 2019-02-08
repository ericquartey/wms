using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Prism.Events;
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
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            }
            var inverterAction = new ActionBlocks.PositioningDrawer();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.PositioningDrawer_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.PositioningDrawer_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.driver;
            inverterAction.Initialize();

            inverterAction.MoveAlongVerticalAxisToPoint(targetPosition, vMax, acc, dec, weight, offset);
            
        }

        private void PositioningDrawer_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            this.inverterAction.ErrorEvent -= this.PositioningDrawer_ThrowErrorEvent;
        }

        private void PositioningDrawer_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.End);
            this.inverterAction.EndEvent -= this.PositioningDrawer_ThrowEndEvent;
        }

        #endregion Methods
    }
}
