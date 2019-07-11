using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMissionCompletedMessageData : IMessageData
    {
        #region Properties

        int BayId { get; set; }

        int MissionId { get; set; }

        #endregion
    }
}
