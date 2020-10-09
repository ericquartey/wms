namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ISocketLinkLaserPointerChangeMessageData : IMessageData
    {
        #region Properties

        int CommandCode { get; }

        int X { get; }

        int Y { get; }

        int Z { get; }

        #endregion
    }
}
