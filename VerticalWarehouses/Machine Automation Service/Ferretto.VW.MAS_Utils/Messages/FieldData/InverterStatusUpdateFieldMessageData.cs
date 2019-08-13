using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterStatusUpdateFieldMessageData : FieldMessageData, IInverterStatusUpdateFieldMessageData
    {
        #region Constructors

        public InverterStatusUpdateFieldMessageData(
            bool sensorStatus,
            int sensorUpdateInterval,
            bool axisPosition,
            int axisUpdateInterval,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.SensorStatus = sensorStatus;
            this.SensorUpdateInterval = sensorUpdateInterval;

            this.AxisPosition = axisPosition;
            this.AxisUpdateInterval = axisUpdateInterval;
        }

        public InverterStatusUpdateFieldMessageData(
            Axis currentAxis,
            bool[] currentSensorStatus,
            int currentPosition,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisPosition = true;
            this.CurrentAxis = currentAxis;
            this.CurrentPosition = currentPosition;

            this.SensorStatus = true;
            this.CurrentSensorStatus = currentSensorStatus;
        }

        public InverterStatusUpdateFieldMessageData(
            bool[] currentSensorStatus,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisPosition = false;

            this.SensorStatus = true;
            this.CurrentSensorStatus = currentSensorStatus;
        }

        public InverterStatusUpdateFieldMessageData(
            Axis currentAxis,
            int currentPosition,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisPosition = true;
            this.CurrentAxis = currentAxis;
            this.CurrentPosition = currentPosition;

            this.SensorStatus = false;
        }

        #endregion

        #region Properties

        public bool AxisPosition { get; }

        public int AxisUpdateInterval { get; }

        public Axis CurrentAxis { get; }

        public int CurrentPosition { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        public bool[] CurrentSensorStatus { get; }

        public bool SensorStatus { get; }

        public int SensorUpdateInterval { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            string currentSensorStatus = string.Empty;
            if (this.CurrentSensorStatus != null)
            {
                var sb = new StringBuilder();
                foreach (var b in this.CurrentSensorStatus)
                {
                    sb.AppendFormat("{0:x2};", b);
                }
                currentSensorStatus = sb.ToString();
            }
            return $"Position:{this.AxisPosition} Interval:{this.AxisUpdateInterval} Position:{this.AxisPosition} CurrentAxis:{this.CurrentAxis} CurrentPosition:{this.CurrentPosition} CurrentSensorStatus:{currentSensorStatus}";
        }

        #endregion
    }
}
