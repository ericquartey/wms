using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class MissionInfo : BaseModel<int>, IMissionPolicy
    {
        #region Properties

        public int? AreaId { get; set; }

        public string AreaName { get; set; }

        public string BayDescription { get; set; }

        public int? BayId { get; set; }

        [JsonIgnore]
        public int CompletedOperationsCount { get; set; }

        public DateTime CreationDate { get; set; }

        [JsonIgnore]
        public int ErrorOperationsCount { get; set; }

        [JsonIgnore]
        public int ExecutingOperationsCount { get; set; }

        [JsonIgnore]
        public int IncompleteOperationsCount { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public string LoadingUnitCode { get; set; }

        public int LoadingUnitId { get; set; }

        [JsonIgnore]
        public int NewOperationsCount { get; set; }

        public IEnumerable<MissionOperationInfo> Operations { get; set; }

        [JsonIgnore]
        public int OperationsCount { get; private set; }

        [Positive]
        public int Priority { get; set; }

        public MissionStatus Status { get; set; }

        #endregion
    }
}
