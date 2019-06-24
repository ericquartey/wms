using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces.Policies;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class MissionWithLoadingUnitDetails : BaseModel<int>, IMissionPolicy
    {
        #region Properties

        public string AisleName { get; set; }

        public string BayDescription { get; set; }

        public int? BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public LoadingUnitMissionInfo LoadingUnit { get; set; }

        public IEnumerable<MissionOperationInfo> Operations { get; set; }

        [Positive]
        public int Priority { get; set; }

        public MissionStatus Status { get; set; }

        #endregion
    }
}
