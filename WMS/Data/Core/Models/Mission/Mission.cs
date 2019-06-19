using System;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces.Policies;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class Mission : BaseModel<int>, IMissionPolicy
    {
        #region Properties

        public string BayDescription { get; set; }

        public int? BayId { get; set; }

        public string CellAisleName { get; set; }

        public int? CellId { get; set; }

        public int? CompartmentId { get; set; }

        [Positive]
        public double? CompartmentTypeHeight { get; set; }

        [Positive]
        public double? CompartmentTypeWidth { get; set; }

        public DateTime CreationDate { get; set; }

        [PositiveOrZero]
        public double DispatchedQuantity { get; set; }

        public string ItemDescription { get; set; }

        public int? ItemId { get; set; }

        public string ItemListDescription { get; set; }

        public int? ItemListId { get; set; }

        public string ItemListRowCode { get; set; }

        public int? ItemListRowId { get; set; }

        public string ItemMeasureUnitDescription { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public string LoadingUnitCode { get; set; }

        public int? LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int? MaterialStatusId { get; set; }

        public string PackageTypeDescription { get; set; }

        public int? PackageTypeId { get; set; }

        [Positive]
        public int Priority { get; set; }

        [PositiveOrZero]
        public double QuantityRemainingToDispatch => this.RequestedQuantity - this.DispatchedQuantity;

        public string RegistrationNumber { get; set; }

        [PositiveOrZero]
        public double RequestedQuantity { get; set; } // TODO: create separate models for different kinds of missions (like SchedulerRequest) and put back this chec to CheckIfStrictlyPositive

        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
