using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteHorizontalHoming()
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.Error, "Inverter action has already defined", Verbosity.Info));
            }
            var inverterAction = new ActionBlocks.CalibrateAxis();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.HorizontalCalibration_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.HorizontalCalibration_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.ActualCalibrationAxis = CalibrationType.HORIZONTAL_CALIBRATION;
            inverterAction.SetAxisOrigin();
        }

        public void ExecuteVerticalHoming()
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.Error, "Inverter action has already defined", Verbosity.Info));
            }
            var inverterAction = new ActionBlocks.CalibrateAxis();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.VerticalCalibration_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.VerticalCalibration_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION;
            inverterAction.SetAxisOrigin();
        }

        private void HorizontalCalibration_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Horizontal Calibration Ended", Verbosity.Info));

            this.inverterAction.EndEvent -= this.HorizontalCalibration_ThrowEndEvent;
        }

        private void HorizontalCalibration_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));

            this.inverterAction.ErrorEvent -= this.HorizontalCalibration_ThrowErrorEvent;
        }

        private void VerticalCalibration_ThrowEndEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Vertical Calibration Ended", Verbosity.Info));

            this.inverterAction.EndEvent -= this.VerticalCalibration_ThrowEndEvent;
        }

        private void VerticalCalibration_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));

            this.inverterAction.ErrorEvent -= this.VerticalCalibration_ThrowErrorEvent;
        }

        #endregion
    }
}
