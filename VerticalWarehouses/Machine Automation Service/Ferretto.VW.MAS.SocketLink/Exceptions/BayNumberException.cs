using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink
{
    public class BayNumberException : Exception
    {
        #region Constructors

        public BayNumberException()
        {
        }

        public BayNumberException(string message) : base(message)
        {
        }

        public BayNumberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public BayNumberException(string message, string bayNumber) : this(message)
        {
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public string BayNumber { get; }

        #endregion
    }
}
