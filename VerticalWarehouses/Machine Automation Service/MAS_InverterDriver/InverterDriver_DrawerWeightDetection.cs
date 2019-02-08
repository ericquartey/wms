using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Prism.Events;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

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
                    this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
                }
                var inverterAction = new ActionBlocks.DrawerWeightDetection();
                this.inverterAction = inverterAction;

                this.inverterAction.EndEvent += this.DrawerWeight_ThrowEndEvent;
                this.inverterAction.ErrorEvent += this.DrawerWeight_ThrowErrorEvent;
                inverterAction.SetInverterDriverInterface = this.driver;
                inverterAction.Initialize();
                inverterAction.Run(targetPosition, vMax, acc, dec);
        }

        private void DrawerWeight_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            this.inverterAction.ErrorEvent -= this.DrawerWeight_ThrowErrorEvent;
        }

        private void DrawerWeight_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.End);
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
