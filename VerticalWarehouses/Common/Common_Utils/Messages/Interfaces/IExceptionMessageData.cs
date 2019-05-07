using System;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IExceptionMessageData : IMessageData
    {
        #region Properties

        string Description { get; }

        Exception InnerException { get; }

        #endregion
    }
}
