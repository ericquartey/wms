using System.Collections.Generic;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class Bay
    {
        #region Properties

        public string ConnectionId { get; set; }

        public MissionInfo CurrentMission { get; set; }

        public MissionOperation CurrentMissionOperation { get; set; }

        public int Id { get; set; }

        public string IpAddress { get; set; }

        public bool IsConnected { get; set; }

        public IEnumerable<MissionInfo> PendingMissions { get; set; } = new List<MissionInfo>();

        public BayStatus Status { get; set; } = BayStatus.Unavailable;

        public BayType Type { get; set; }

        #endregion
    }
}
