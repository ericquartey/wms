using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class CalibrateAxisMessageData : ICalibrateAxisMessageData
    {
        #region Constructors

        public CalibrateAxisMessageData(Axis axisToCalibrate, int currentStep, int maxStep, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.Verbosity = verbosity;
            this.CurrentStepCalibrate = currentStep + 1;
            this.MaxStepCalibrate = maxStep;
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
