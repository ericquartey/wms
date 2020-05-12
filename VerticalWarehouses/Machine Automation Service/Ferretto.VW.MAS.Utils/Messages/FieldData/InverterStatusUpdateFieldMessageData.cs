using System.Text;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;


namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterStatusUpdateFieldMessageData : FieldMessageData, IInverterStatusUpdateFieldMessageData
    {
        #region Constructors

        public InverterStatusUpdateFieldMessageData(
            DataSample torqueCurrent,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.TorqueCurrent = torqueCurrent;
        }

        public InverterStatusUpdateFieldMessageData(
            Axis axis,
            bool[] currentSensorStatus,
            double currentPosition,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.CurrentAxis = axis;
            this.CurrentPosition = currentPosition;

            this.CurrentSensorStatus = currentSensorStatus;
        }

        public InverterStatusUpdateFieldMessageData(
            bool[] currentSensorStatus,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.CurrentSensorStatus = currentSensorStatus;
        }

        #endregion

        #region Properties

        public Axis CurrentAxis { get; }

        public double? CurrentPosition { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        public bool[] CurrentSensorStatus { get; }

        public DataSample TorqueCurrent { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            var currentSensorStatus = string.Empty;
            if (this.CurrentSensorStatus != null)
            {
                var sb = new StringBuilder();
                foreach (var b in this.CurrentSensorStatus)
                {
                    sb.AppendFormat("{0:x2};", b);
                }

                currentSensorStatus = sb.ToString();
            }

            return $"CurrentAxis:{this.CurrentAxis} CurrentPosition:{this.CurrentPosition} CurrentSensorStatus:{currentSensorStatus}";
        }

        #endregion
    }
}
