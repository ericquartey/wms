using System;
using System.Collections.Generic;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteHorizontalPosition(Int32 target, Int32 speed, Int32 direction, List<ProfilePosition> profile)
        {
            if (this.inverterAction != null)
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver,
                    MessageType.Homing, MessageStatus.OperationError,
                    ErrorLevel.Error));
            var inverterAction = new HorizontalMovingDrawer();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.HorizontalPosition_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.HorizontalPosition_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.Run(target, speed, direction, profile);
        }

        private void HorizontalPosition_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Horizontal position Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationEnd));

            this.inverterAction.EndEvent -= this.HorizontalPosition_ThrowEndEvent;
        }

        private void HorizontalPosition_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationError,
                ErrorLevel.Error));

            this.inverterAction.ErrorEvent -= this.HorizontalPosition_ThrowErrorEvent;
        }

        #endregion
    }
}
