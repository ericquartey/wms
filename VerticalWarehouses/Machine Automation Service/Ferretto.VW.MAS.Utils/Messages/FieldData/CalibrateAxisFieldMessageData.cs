using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;


namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class CalibrateAxisFieldMessageData : FieldMessageData, ICalibrateAxisFieldMessageData
    {
        #region Constructors

        public CalibrateAxisFieldMessageData(Axis axisToCalibrate, Calibration calibration, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.CalibrationType = calibration;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; }

        public Calibration CalibrationType { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToCalibrate:{this.AxisToCalibrate.ToString()}";
        }

        #endregion
    }
}
