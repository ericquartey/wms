namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IWebApiExceptionMessageData : IMessageData
    {
        #region Properties

        int ExceptionCode { get; }

        string ExceptionDescription { get; }

        #endregion
    }
}
