using System;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IIoExceptionFieldMessageData : IFieldMessageData
    {
        #region Properties

        int ExceptionCode { get; }

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
