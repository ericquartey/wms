using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterStatusUpdateFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool AxisPosition { get; }

        int AxisUpdateInterval { get; }

        Axis CurrentAxis { get; }

        int CurrentPosition { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        bool[] CurrentSensorStatus { get; }

        bool SensorStatus { get; }

        int SensorUpdateInterval { get; }

        #endregion
    }
}
