namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IErrorStatusMessageData : IMessageData
    {
        #region Properties

        int ErrorId { get; }

        #endregion
    }
}
