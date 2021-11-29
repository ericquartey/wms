using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class HomingMessageData : IHomingMessageData
    {
        #region Constructors

        public HomingMessageData()
        {
        }

        public HomingMessageData(Axis axisToCalibrate, Calibration calibration, int? loadingUnitId, bool showErrors, bool turnBack, bool calibrateFromPPC = false, bool bypassSensor = false, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.CalibrationType = calibration;
            this.LoadingUnitId = loadingUnitId;
            this.ShowErrors = showErrors;
            this.TurnBack = turnBack;
            this.CalibrateFromPPC = calibrateFromPPC;
            this.BypassSensor = bypassSensor;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; set; }

        public bool BypassSensor { get; set; }

        public bool CalibrateFromPPC { get; set; }

        public Calibration CalibrationType { get; set; }

        public int? LoadingUnitId { get; set; }

        public bool ShowErrors { get; }

        public bool TurnBack { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToCalibrate:{this.AxisToCalibrate.ToString()}";
        }

        #endregion
    }
}
