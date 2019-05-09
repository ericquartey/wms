using System;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IExceptionMessageData : IMessageData
    {
        #region Properties

        int ExceptionCode { get; }

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
