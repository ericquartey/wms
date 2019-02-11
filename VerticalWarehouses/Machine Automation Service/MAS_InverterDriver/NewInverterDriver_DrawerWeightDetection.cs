using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.EventParameters;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Fields

        private float weight;

        #endregion Fields


        #region Methods

        public void ExecuteDrawerWeight(int targetPosition, float vMax, float acc, float dec)
        {
                this.weight = -1.0f;
                if (this.inverterAction != null)
                {
                  this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                      (OperationType.Homing, OperationStatus.Error, "Inverter action has already defined", Verbosity.Info));
                }
                var inverterAction = new ActionBlocks.DrawerWeightDetection();
                this.inverterAction = inverterAction;

                this.inverterAction.EndEvent += this.DrawerWeight_ThrowEndEvent;
                this.inverterAction.ErrorEvent += this.DrawerWeight_ThrowErrorEvent;
                inverterAction.SetInverterDriverInterface = this.inverterDriver;
                inverterAction.Initialize();
                inverterAction.Run(targetPosition, vMax, acc, dec);
        }

        private void DrawerWeight_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));

            this.inverterAction.ErrorEvent -= this.DrawerWeight_ThrowErrorEvent;
        }

        private void DrawerWeight_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Drawer weight detection Ended", Verbosity.Info));

            this.inverterAction.EndEvent -= this.DrawerWeight_ThrowEndEvent;
            if(this.inverterAction is ActionBlocks.DrawerWeightDetection)
            {
                this.weight = ((ActionBlocks.DrawerWeightDetection)this.inverterAction).Weight;
            }
            
        }

        public float GetDrawerWeight
        {
            get
            {
                return this.weight;
            }
        }


        #endregion Methods
    }
}
