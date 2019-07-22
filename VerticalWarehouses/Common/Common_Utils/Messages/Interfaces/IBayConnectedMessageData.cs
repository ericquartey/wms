using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IBayConnectedMessageData : IMessageData
    {
        #region Properties

        int BayId { get; set; }

        BayType BayType { get; set; }

        int PendingMissionsCount { get; set; }

        #endregion
    }
}
