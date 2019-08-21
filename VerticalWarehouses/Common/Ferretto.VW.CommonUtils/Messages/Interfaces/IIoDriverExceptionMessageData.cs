using System;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IIoDriverExceptionMessageData : IExceptionMessageData, IMessageData
    {
        #region Properties

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
