using System.Collections.Generic;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteHorizontalPosition(int target, int speed, int direction, List<ProfilePosition> profile, float weight)
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.Error, "Inverter action has already defined", Verbosity.Info));
            }
            var inverterAction = new HorizontalMovingDrawer();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.HorizontalPosition_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.HorizontalPosition_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.Run(target, speed, direction, profile);
        }

        public void ExecuteHorizontalPositionStop()
        {
        }

        private void HorizontalPosition_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Horizontal position Ended", Verbosity.Info));

            this.inverterAction.EndEvent -= this.HorizontalPosition_ThrowEndEvent;
        }

        private void HorizontalPosition_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));

            this.inverterAction.ErrorEvent -= this.HorizontalPosition_ThrowErrorEvent;
        }

        #endregion
    }
}
