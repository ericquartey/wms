using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class NewConnectedClientMessageData : INewConnectedClientMessageData
    {
        #region Properties

        public string ConnectionId { get; set; }

        public string LocalIPAddress { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
