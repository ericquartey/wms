using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteHomingStop()
        {
            if (this.inverterAction == null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.Error, "Stop Homing operation failed", Verbosity.Info));
            }

            ((ActionBlocks.CalibrateAxis)this.inverterAction).StopInverter();
            if (((ActionBlocks.CalibrateAxis)this.inverterAction).ActualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
            {
                this.inverterAction.EndEvent -= this.HorizontalCalibration_ThrowEndEvent;
                this.inverterAction.ErrorEvent -= this.HorizontalCalibration_ThrowErrorEvent;
            }
            else
            {
                this.inverterAction.EndEvent -= this.VerticalCalibration_ThrowEndEvent;
                this.inverterAction.ErrorEvent -= this.VerticalCalibration_ThrowErrorEvent;
            }
            this.inverterAction = null;
        }

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
            this.inverterAction.EndEvent -= this.HorizontalCalibration_ThrowEndEvent;
            ((ActionBlocks.CalibrateAxis)this.inverterAction).Terminate();
            this.inverterAction = null;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Horizontal Calibration Ended", Verbosity.Info));
        }

        private void HorizontalCalibration_ThrowErrorEvent()
        {
            this.inverterAction.ErrorEvent -= this.HorizontalCalibration_ThrowErrorEvent;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));
        }

        private void VerticalCalibration_ThrowEndEvent()
        {
            this.inverterAction.EndEvent -= this.VerticalCalibration_ThrowEndEvent;
            ((ActionBlocks.CalibrateAxis)this.inverterAction).Terminate();
            this.inverterAction = null;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.End, "Vertical Calibration Ended", Verbosity.Info));
        }

        private void VerticalCalibration_ThrowErrorEvent()
        {
            this.inverterAction.ErrorEvent -= this.VerticalCalibration_ThrowErrorEvent;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));
        }

        #endregion
    }
}
