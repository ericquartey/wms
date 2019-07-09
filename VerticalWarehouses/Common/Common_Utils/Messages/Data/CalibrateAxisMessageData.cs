using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class CalibrateAxisMessageData : ICalibrateAxisMessageData
    {
        #region Constructors

        public CalibrateAxisMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
