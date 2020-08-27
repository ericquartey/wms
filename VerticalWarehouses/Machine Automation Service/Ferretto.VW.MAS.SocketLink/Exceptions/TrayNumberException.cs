using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink
{
    public class TrayNumberException : Exception
    {
        #region Constructors

        public TrayNumberException()
        {
        }

        public TrayNumberException(string message) : base(message)
        {
        }

        public TrayNumberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TrayNumberException(string message, string trayNumber) : this(message)
        {
            this.TrayNumber = trayNumber;
        }

        #endregion

        #region Properties

        public string TrayNumber { get; }

        #endregion
    }
}
