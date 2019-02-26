using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver;
using CalibrateAxis = Ferretto.VW.MAS_InverterDriver.ActionBlocks.CalibrateAxis;

namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class NewInverterDriver
    {
        #region Methods

        public void ExecuteHomingStop()
        {
            if (this.inverterAction == null)
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Stop Homing operation failed", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                    MessageStatus.OperationError,
                    ErrorLevel.Error));

            ( (CalibrateAxis) this.inverterAction ).StopInverter();
            if (( (CalibrateAxis) this.inverterAction ).ActualCalibrationAxis == CalibrationType.VERTICAL_CALIBRATION)
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
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver,
                    MessageType.Homing, MessageStatus.OperationError,
                    ErrorLevel.Error));
            var inverterAction = new CalibrateAxis();
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
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                    "Inverter action has already defined", MessageActor.Any, MessageActor.InverterDriver,
                    MessageType.Homing, MessageStatus.OperationError,
                    ErrorLevel.Error));
            var inverterAction = new CalibrateAxis();
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
            ( (CalibrateAxis) this.inverterAction ).Terminate();
            this.inverterAction = null;

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Horizontal Calibration Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationEnd));
        }

        private void HorizontalCalibration_ThrowErrorEvent()
        {
            this.inverterAction.ErrorEvent -= this.HorizontalCalibration_ThrowErrorEvent;

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationError,
                ErrorLevel.Error));
        }

        private void VerticalCalibration_ThrowEndEvent()
        {
            this.inverterAction.EndEvent -= this.VerticalCalibration_ThrowEndEvent;
            ( (CalibrateAxis) this.inverterAction ).Terminate();
            this.inverterAction = null;

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Vertical Calibration Ended", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationEnd));
        }

        private void VerticalCalibration_ThrowErrorEvent()
        {
            this.inverterAction.ErrorEvent -= this.VerticalCalibration_ThrowErrorEvent;

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Internal inverter driver error", MessageActor.Any, MessageActor.InverterDriver, MessageType.Homing,
                MessageStatus.OperationError,
                ErrorLevel.Error));
        }

        #endregion
    }
}
