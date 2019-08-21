namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IWebApiExceptionMessageData : IExceptionMessageData, IMessageData
    {
        #region Properties

        string ExceptionDescription { get; }

        #endregion
    }
}
