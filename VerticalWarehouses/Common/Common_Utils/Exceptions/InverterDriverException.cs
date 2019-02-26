using System;
using System.Runtime.Serialization;

namespace Ferretto.VW.Common_Utils.Exceptions
{
    public enum InverterDriverExceptionCode
    {
        SocketOpen = 1,

        RemoteEndPointCreationFailure,

        TcpClientCreationFailed,

        TcpInverterConnectionFailed,

        GetNetworkStreamFailed,

        UninitializedNetworkStream,

        MisconfiguredNetworkStream,

        NetworkStreamReadFailure,

        NetworkStreamWriteFailure
    }

    public class InverterDriverException : Exception
    {
        #region Constructors

        public InverterDriverException()
        {
        }

        public InverterDriverException(String message) : base(message)
        {
        }

        public InverterDriverException(String message, InverterDriverExceptionCode exceptionEnum) : base(message)
        {
            this.InverterDriverExceptionCode = exceptionEnum;
        }

        public InverterDriverException(String message, Exception inner) : base(message, inner)
        {
        }

        public InverterDriverException(String message, InverterDriverExceptionCode exceptionEnum, Exception inner) :
            base(message, inner)
        {
            this.InverterDriverExceptionCode = exceptionEnum;
        }

        protected InverterDriverException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public InverterDriverExceptionCode InverterDriverExceptionCode { get; protected set; }

        #endregion
    }
}
