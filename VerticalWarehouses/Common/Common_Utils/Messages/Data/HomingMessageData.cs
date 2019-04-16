using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class HomingMessageData : IHomingMessageData
    {
        #region Constructors

        public HomingMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
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
