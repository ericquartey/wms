using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class CalibrateAxisMessageData : ICalibrateAxisMessageData
    {
        #region Constructors

        public CalibrateAxisMessageData()
        {
        }

        public CalibrateAxisMessageData(
            Axis axisToCalibrate,
            int currentStepCalibrate,
            int maxStepCalibrate,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.Verbosity = verbosity;
            this.CurrentStepCalibrate = currentStepCalibrate;
            this.MaxStepCalibrate = maxStepCalibrate;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; }

        public int CurrentStepCalibrate { get; }

        public int MaxStepCalibrate { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToCalibrate:{this.AxisToCalibrate.ToString()} CurrentStep:{this.CurrentStepCalibrate} MaxStep:{this.MaxStepCalibrate}";
        }

        #endregion
    }
}
