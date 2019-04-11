using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface ICalibrateAxisFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        #endregion
    }
}
