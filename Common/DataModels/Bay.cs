using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Baia
    public sealed class Bay
    {
        #region Properties

        public Area Area { get; set; }
        public int AreaId { get; set; }
        public BayType BayType { get; set; }
        public string BayTypeId { get; set; }
        public string Description { get; set; }
        public List<Mission> DestinationMissions { get; set; }
        public int Id { get; set; }
        public int? LoadingUnitsBufferSize { get; set; }
        public Machine Machine { get; set; }
        public int? MachineId { get; set; }
        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }
        public List<Mission> SourceMissions { get; set; }

        #endregion Properties
    }
}
