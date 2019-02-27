using System;
using System.Runtime.Serialization;
using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Exceptions
{
    public class IoDriverException : Exception
    {
        #region Constructors

        public IoDriverException()
        {
        }

        public IoDriverException(string message) : base(message)
        {
        }

        public IoDriverException(string message, IoDriverExceptionCode exceptionEnum) : base(message)
        {
            this.IoDriverExceptionCode = exceptionEnum;
        }

        public IoDriverException(string message, Exception inner) : base(message, inner)
        {
        }

        public IoDriverException(string message, IoDriverExceptionCode exceptionEnum, Exception inner) :
            base(message, inner)
        {
            this.IoDriverExceptionCode = exceptionEnum;
        }

        protected IoDriverException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public IoDriverExceptionCode IoDriverExceptionCode { get; protected set; }

        #endregion
    }
}
