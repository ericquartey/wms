using System;
using System.Runtime.Serialization;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Exceptions
{
    public class FiniteStateMachinesException : Exception
    {
        #region Constructors

        public FiniteStateMachinesException()
        {
        }

        public FiniteStateMachinesException(string message) : base(message)
        {
        }

        public FiniteStateMachinesException(string message, FiniteStateMachinesExceptionCode exceptionEnum) : base(message)
        {
            this.FiniteStateMachinesExceptionCode = exceptionEnum;
        }

        public FiniteStateMachinesException(string message, Exception inner) : base(message, inner)
        {
        }

        public FiniteStateMachinesException(string message, FiniteStateMachinesExceptionCode exceptionEnum, Exception inner) :
            base(message, inner)
        {
            this.FiniteStateMachinesExceptionCode = exceptionEnum;
        }

        protected FiniteStateMachinesException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public FiniteStateMachinesExceptionCode FiniteStateMachinesExceptionCode { get; protected set; }

        #endregion
    }
}
