using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface ICalibrateMessageData : IMessageData
    {
        #region Properties

        float Acceleration { get; }

        Axis AxisToCalibrate { get; }

        float CreepSpeed { get; }

        float FastSpeed { get; }

        #endregion
    }
}
