using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterStatusUpdateFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool AxisPosition { get; }

        int AxisUpdateInterval { get; }

        Axis CurrentAxis { get; }

        int CurrentPosition { get; }

        bool[] CurrentSensorStatus { get; }

        bool SensorStatus { get; }

        int SensorUpdateInterval { get; }

        #endregion
    }
}
