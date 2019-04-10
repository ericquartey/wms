using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Axis = Ferretto.VW.MAS_Utils.Enumerations.Axis;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
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
