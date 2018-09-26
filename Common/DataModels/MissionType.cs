using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Missione
    public sealed class MissionType
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public List<Mission> Missions { get; set; }
    }
}
