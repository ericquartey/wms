using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MissionDetails : BaseModel<int>
    {
        #region Properties

        public int? BayId { get; set; }

        public int? CompartmentId { get; set; }

        public DateTime CreationDate { get; set; }

        public ItemMissionInfo Item { get; set; }

        public ItemListMissionInfo ItemList { get; set; }

        public ItemListRowMissionInfo ItemListRow { get; set; }

        public LoadingUnitMissionInfo LoadingUnit { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int? MaterialStatusId { get; set; }

        public string PackageTypeDescription { get; set; }

        public int? PackageTypeId { get; set; }

        [Positive]
        public int Priority { get; set; }

        public string RegistrationNumber { get; set; }

        [PositiveOrZero]
        public double RequestedQuantity { get; set; } // TODO: create separate models for different kinds of missions (like SchedulerRequest) and put back this chec to CheckIfStrictlyPositive

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
