using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;


namespace Ferretto.VW.CommonUtils.Messages.Data
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

        #region Methods

        public override string ToString()
        {
            return $"AxisToStop:{this.AxisToStop}";
        }

        #endregion
    }
}
