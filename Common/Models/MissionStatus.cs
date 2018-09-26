using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Missione
    public sealed class MissionStatus
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public List<Mission> Missions { get; set; }
    }
}
