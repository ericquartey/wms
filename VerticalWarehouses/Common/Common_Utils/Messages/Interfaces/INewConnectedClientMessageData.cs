using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface INewConnectedClientMessageData : IMessageData
    {
        #region Properties

        string ConnectionId { get; set; }

        string LocalIPAddress { get; set; }

        #endregion
    }
}
