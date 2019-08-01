using System;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IDLExceptionMessageData : IExceptionMessageData, IMessageData
    {
        #region Properties

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
