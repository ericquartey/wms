using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
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
