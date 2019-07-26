using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface ICalibrateAxisFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        #endregion
    }
}
