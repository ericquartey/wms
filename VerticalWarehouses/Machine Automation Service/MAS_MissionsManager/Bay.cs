using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.MissionsManager
{
    public class Bay
    {
        #region Properties

        public string ConnectionId { get; set; }

        public MissionInfo CurrentMission { get; set; }

        public MissionOperationInfo CurrentMissionOperation { get; set; }

        public int Id { get; set; }

        public System.Net.IPAddress IpAddress { get; set; }

        public IEnumerable<MissionInfo> PendingMissions { get; set; } = new List<MissionInfo>();

        public BayStatus Status { get; set; } = BayStatus.Unavailable;

        public BayType Type { get; set; }

        #endregion
    }
}
