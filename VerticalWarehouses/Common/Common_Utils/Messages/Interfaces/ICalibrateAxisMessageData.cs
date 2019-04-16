using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface ICalibrateAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        #endregion
    }
}
