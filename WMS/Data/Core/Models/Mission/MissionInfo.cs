using System;
using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces.Policies;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class MissionInfo : BaseModel<int>, IMissionPolicy
    {
        #region Properties

        public string BayDescription { get; set; }

        public int? BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public string LoadingUnitCode { get; set; }

        public int LoadingUnitId { get; set; }

        public IEnumerable<MissionOperationInfo> Operations { get; set; }

        [Positive]
        public int Priority { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.New;

        #endregion
    }
}
