namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ISocketLinkAlphaNumericBarChangeMessageData : IMessageData
    {
        #region Properties

        int CommandCode { get; }

        string TextMessage { get; }

        int X { get; }

        #endregion
    }
}
