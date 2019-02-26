namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public enum Axis
    {
        Horizontal,

        Vertical,

        Both
    }

    public interface ICalibrateMessageData : IMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        #endregion
    }
}
