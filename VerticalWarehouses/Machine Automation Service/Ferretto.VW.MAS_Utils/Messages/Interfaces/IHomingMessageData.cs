using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IHomingMessageData : IMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        FieldNotificationMessage FieldMessage { get; }

        #endregion
    }
}
