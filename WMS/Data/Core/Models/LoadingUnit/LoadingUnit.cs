using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnit))]
    public class LoadingUnit : BaseModel<int>, ILoadingUnitDeletePolicy, ILoadingUnitWithdrawPolicy, ILoadingUnitUpdatePolicy
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ActiveMissionsCount { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ActiveSchedulerRequestsCount { get; set; }

        public string AisleName { get; set; }

        [PositiveOrZero]
        public double? AreaFillRate { get; set; }

        public string AreaName { get; set; }

        public int? CellColumn { get; set; }

        public int? CellFloor { get; set; }

        public int? CellId { get; set; }

        public int? CellNumber { get; set; }

        public string CellPositionDescription { get; set; }

        public Side? CellSide { get; set; }

        [Required]
        public string Code { get; set; }

        [PositiveOrZero]
        public int CompartmentsCount { get; set; }

        public string LoadingUnitStatusDescription { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        #endregion
    }
}
