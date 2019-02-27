using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    internal class CalibrateMessageData : ICalibrateMessageData
    {
        #region Constructors

        public CalibrateMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.Verbosity = verbosity;
        }

        public CalibrateMessageData(Axis axisToCalibrate, float fastSpeed, float creepSpeed, float acceleration, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.Verbosity = verbosity;
            this.Acceleration = acceleration;
            this.FastSpeed = fastSpeed;
            this.CreepSpeed = creepSpeed;
        }

        #endregion

        #region Properties

        public float Acceleration { get; private set; }

        public Axis AxisToCalibrate { get; }

        public float CreepSpeed { get; private set; }

        public float FastSpeed { get; private set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
