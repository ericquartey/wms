using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class CalibrateAxisFieldMessageData : ICalibrateAxisFieldMessageData
    {
        #region Constructors

        public CalibrateAxisFieldMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToCalibrate:{this.AxisToCalibrate.ToString()}";
        }

        #endregion
    }
}
