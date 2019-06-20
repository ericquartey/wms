using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IMissionCompletedMessageData : IMessageData
    {
        #region Properties

        int BayId { get; set; }

        int MissionId { get; set; }

        #endregion
    }
}
