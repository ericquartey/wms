using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface ICalibrateAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        #endregion
    }
}
