using System;
using System.Runtime.Serialization;
using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Exceptions
{
    public class SocketTransportException : Exception
    {
        #region Constructors

        public SocketTransportException()
        {
        }

        public SocketTransportException(string message) : base(message)
        {
        }

        public SocketTransportException(string message, SocketTransportExceptionCode exceptionEnum) : base(message)
        {
            this.ExceptionCode = exceptionEnum;
        }

        public SocketTransportException(string message, Exception inner) : base(message, inner)
        {
        }

        public SocketTransportException(string message, SocketTransportExceptionCode exceptionEnum, Exception inner) :
            base(message, inner)
        {
            this.ExceptionCode = exceptionEnum;
        }

        protected SocketTransportException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public SocketTransportExceptionCode ExceptionCode { get; protected set; }

        #endregion
    }
}
