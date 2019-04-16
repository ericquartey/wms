using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterStatusUpdateFieldMessageData : IInverterStatusUpdateFieldMessageData
    {
        #region Constructors

        public InverterStatusUpdateFieldMessageData(bool sensorStatus, int sensorUpdateInterval, bool axisPosition, int axisUpdateInterval, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.SensorStatus = sensorStatus;
            this.SensorUpdateInterval = sensorUpdateInterval;

            this.AxisPosition = axisPosition;
            this.AxisUpdateInterval = axisUpdateInterval;

            this.Verbosity = verbosity;
        }

        public InverterStatusUpdateFieldMessageData(Axis currentAxis, bool[] currentSensorStatus, int currentPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisPosition = true;
            this.CurrentAxis = currentAxis;
            this.CurrentPosition = currentPosition;

            this.SensorStatus = true;
            this.CurrentSensorStatus = currentSensorStatus;

            this.Verbosity = verbosity;
        }

        public InverterStatusUpdateFieldMessageData(bool[] currentSensorStatus, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisPosition = false;

            this.SensorStatus = true;
            this.CurrentSensorStatus = currentSensorStatus;

            this.Verbosity = verbosity;
        }

        public InverterStatusUpdateFieldMessageData(Axis currentAxis, int currentPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisPosition = true;
            this.CurrentAxis = currentAxis;
            this.CurrentPosition = currentPosition;

            this.SensorStatus = false;

            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public bool AxisPosition { get; }

        public int AxisUpdateInterval { get; }

        public Axis CurrentAxis { get; }

        public int CurrentPosition { get; }

        public bool[] CurrentSensorStatus { get; }

        public bool SensorStatus { get; }

        public int SensorUpdateInterval { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
