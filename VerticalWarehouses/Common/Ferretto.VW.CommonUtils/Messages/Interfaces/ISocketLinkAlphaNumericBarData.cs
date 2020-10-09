namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ISocketLinkAlphaNumericBarData : IMessageData
    {
        #region Properties

        int CommandCode { get; }

        string TextMessage { get; }

        int X { get; }

        #endregion
    }
}
