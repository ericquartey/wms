using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.Common_Utils.EventParameters;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteVerticalHoming()
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.Error, "Inverter action has already defined", Verbosity.Info));
            }
            var inverterAction = new ActionBlocks.CalibrateAxis();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.Calibration_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.Calibration_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION;
            inverterAction.SetAxisOrigin();
        }

        public void ExecuteHorizontalHoming()
        {
            if (this.inverterAction != null)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish( new Notification_EventParameter
                    (OperationType.Homing,OperationStatus.Error,"Inverter action has already defined", Verbosity.Info));
            }
            var inverterAction = new ActionBlocks.CalibrateAxis();
            this.inverterAction = inverterAction;

            this.inverterAction.EndEvent += this.Calibration_ThrowEndEvent;
            this.inverterAction.ErrorEvent += this.Calibration_ThrowErrorEvent;
            inverterAction.SetInverterDriverInterface = this.inverterDriver;
            inverterAction.Initialize();

            inverterAction.ActualCalibrationAxis = CalibrationType.HORIZONTAL_CALIBRATION;
            inverterAction.SetAxisOrigin();
        }

        private void Calibration_ThrowErrorEvent()
        {
            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                (OperationType.Homing, OperationStatus.Error, "Internal inverter driver error", Verbosity.Info));

            this.inverterAction.ErrorEvent -= this.Calibration_ThrowErrorEvent;
        }

        private void Calibration_ThrowEndEvent()
        {
            if (((ActionBlocks.CalibrateAxis)this.inverterAction).ActualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.End, "Vertival Calibration Ended", Verbosity.Info));
            }
            else
            {
                this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(new Notification_EventParameter
                    (OperationType.Homing, OperationStatus.End, "Horizontal Calibration Ended", Verbosity.Info));
            }

            this.inverterAction.EndEvent -= this.Calibration_ThrowEndEvent;
        }

        #endregion
    }
}
