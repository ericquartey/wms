using Ferretto.VW.Common_Utils.Events;
using System.Collections.Generic;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Prism.Events;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteHorizontalPosition(int target, int speed, int direction, List<ProfilePosition> profile)
        {
            if(this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            }
            var inverterAction = new ActionBlocks.HorizontalMovingDrawer();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.HorizontalPosition_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.HorizontalPosition_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.driver;
            inverterAction.Initialize();

            inverterAction.Run(target, speed, direction, profile);
            
        }

        private void HorizontalPosition_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            this.inverterAction.ErrorEvent -= this.HorizontalPosition_ThrowErrorEvent;
        }

        private void HorizontalPosition_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.End);
            this.inverterAction.EndEvent -= this.HorizontalPosition_ThrowEndEvent;
        }

        #endregion Methods
    }
}
