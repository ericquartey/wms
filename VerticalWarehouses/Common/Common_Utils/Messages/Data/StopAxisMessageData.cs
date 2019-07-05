using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class StopAxisMessageData : IStopAxisMessageData
    {
        #region Constructors

        public StopAxisMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToStop = axisToCalibrate;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToStop { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
