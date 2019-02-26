using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Properties

        public Single GetDrawerWeight { get; private set; }

        #endregion

        #region Methods

        public void ExecuteDrawerWeight(Int32 targetPosition, Single vMax, Single acc, Single dec)
        {
            this.GetDrawerWeight = -1.0f;
            if (this.inverterAction != null)
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver,
                    MessageType.Homing, MessageStatus.OperationError,
                    ErrorLevel.Error));
            var inverterAction = new DrawerWeightDetection();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.DrawerWeight_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.DrawerWeight_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();
            inverterAction.Run(targetPosition, vMax, acc, dec);
        }

        private void DrawerWeight_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Drawer weight detection Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationEnd));

            this.inverterAction.EndEvent -= this.DrawerWeight_ThrowEndEvent;
            if (this.inverterAction is DrawerWeightDetection)
                this.GetDrawerWeight = ( (DrawerWeightDetection) this.inverterAction ).Weight;
        }

        private void DrawerWeight_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationError,
                ErrorLevel.Error));

            this.inverterAction.ErrorEvent -= this.DrawerWeight_ThrowErrorEvent;
        }

        #endregion
    }
}
