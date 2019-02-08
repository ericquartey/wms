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

        public void ExecuteVerticalHoming()
        {
            if(this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
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

        public void ExecuteHorizontalHoming()
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            }
            var inverterAction = new ActionBlocks.CalibrateAxis();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.Calibration_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.Calibration_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.driver;
            inverterAction.Initialize();

            inverterAction.ActualCalibrationAxis = CalibrationType.HORIZONTAL_CALIBRATION;
            inverterAction.SetAxisOrigin();
        }

        private void Calibration_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.Error);
            this.inverterAction.ErrorEvent -= this.Calibration_ThrowErrorEvent;
        }

        private void Calibration_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(InverterDriver_Notification.End);
            this.inverterAction.EndEvent -= this.Calibration_ThrowEndEvent;
        }

        #endregion Methods
    }
}
