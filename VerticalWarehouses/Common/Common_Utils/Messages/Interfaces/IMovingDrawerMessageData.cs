namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public enum MovingDrawer
    {
        Horizontal,

        Vertical,

    }

    public interface IMovingDrawerMessageData : IMessageData
    {
        #region Properties

        MovingDrawer MovingDrawer { get; }

        #endregion
    }
}
