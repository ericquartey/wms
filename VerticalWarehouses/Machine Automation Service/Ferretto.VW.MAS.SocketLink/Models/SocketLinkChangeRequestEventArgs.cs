using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink.Models
{
    internal class SocketLinkChangeRequestEventArgs : EventArgs
    {
        #region Constructors

        public SocketLinkChangeRequestEventArgs(bool enable)
        {
            this.Enable = enable;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        #endregion
    }
}
