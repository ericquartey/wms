using System;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IInverterExceptionMessageData : IMessageData
    {
        #region Properties

        int ExceptionCode { get; }

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
