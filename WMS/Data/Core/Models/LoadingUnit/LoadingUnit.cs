using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnit : BaseModel<int>, ILoadingUnitDeletePolicy
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        [JsonIgnore]
        public int ActiveMissionsCount { get; set; }

        [JsonIgnore]
        public int ActiveSchedulerRequestsCount { get; set; }

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        public int? CellColumn { get; set; }

        public int? CellFloor { get; set; }

        public int? CellNumber { get; set; }

        public string CellPositionDescription { get; set; }

        public Side? CellSide { get; set; }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public string LoadingUnitStatusDescription { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        #endregion
    }
}
