using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ICalibrateAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        int CurrentStepCalibrate { get; }

        int MaxStepCalibrate { get; }

        #endregion
    }
}
