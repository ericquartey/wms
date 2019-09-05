using System;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IFsmExceptionMessageData : IExceptionMessageData, IMessageData
    {
        #region Properties

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
