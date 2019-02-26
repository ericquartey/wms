using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Fields

        private float weight;

        #endregion


        #region Methods

        public void ExecuteDrawerWeight(int targetPosition, float vMax, float acc, float dec)
        {
                this.weight = -1.0f;
                if (this.inverterAction != null)
                {
                  this.eventAggregator.GetEvent<NotificationEvent>().Publish( new NotificationMessage( null,
                      "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationError,
                      MessageVerbosity.Info, ErrorLevel.Error) );
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
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationError,
                MessageVerbosity.Info, ErrorLevel.Error));

            this.inverterAction.ErrorEvent -= this.DrawerWeight_ThrowErrorEvent;
        }

        private void DrawerWeight_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Drawer weight detection Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationEnd,
                MessageVerbosity.Info));

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


        #endregion
    }
}
