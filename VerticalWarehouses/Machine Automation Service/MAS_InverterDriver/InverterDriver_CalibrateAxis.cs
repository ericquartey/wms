using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.Common_Utils.EventParameters;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class InverterDriver
    {
        #region Methods

        public void ExecuteVerticalHoming()
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter());
            }
            var inverterAction = new ActionBlocks.CalibrateAxis();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.Calibration_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.Calibration_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.driver;
            inverterAction.Initialize();

            inverterAction.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION;
            inverterAction.SetAxisOrigin();
        }

        private void Calibration_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter());
            this.inverterAction.ErrorEvent -= this.Calibration_ThrowErrorEvent;
        }

        private void Calibration_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter());
            this.inverterAction.EndEvent -= this.Calibration_ThrowEndEvent;
        }

        #endregion Methods
    }
}
