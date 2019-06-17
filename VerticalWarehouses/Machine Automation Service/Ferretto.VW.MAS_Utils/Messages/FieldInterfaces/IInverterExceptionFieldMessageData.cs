using System;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterExceptionFieldMessageData : IFieldMessageData
    {
        #region Properties

        int ExceptionCode { get; }

        string ExceptionDescription { get; }

        Exception InnerException { get; }

        #endregion
    }
}
