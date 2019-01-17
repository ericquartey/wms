using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Area
    public sealed class Area : IDataModel
    {
        #region Properties

        public IEnumerable<Aisle> Aisles { get; set; }

        public IEnumerable<ItemArea> AreaItems { get; set; }

        public IEnumerable<Bay> Bays { get; set; }

        public int Id { get; set; }

        public IEnumerable<LoadingUnitRange> LoadingUnitRanges { get; set; }

        public string Name { get; set; }

        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        #endregion Properties
    }
}
