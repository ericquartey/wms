using System;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MissionOperationInfo : BaseModel<int>, IMissionOperationPolicy
    {
        #region Properties

        public double CompartmentDepth { get; set; }

        public double CompartmentWidth { get; set; }

        public DateTime CreationDate { get; set; }

        [PositiveOrZero]
        public double DispatchedQuantity { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public string ItemListCode { get; set; }

        public string ItemListDescription { get; set; }

        public int? ItemListId { get; set; }

        public string ItemListRowCode { get; set; }

        public string ItemListShipmentUnitCode { get; set; }

        public string ItemListShipmentUnitDescription { get; set; }

        public string ItemMeasureUnitDescription { get; set; }

        public string ItemNotes { get; set; }

        public DateTime LastModificationDate { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int MissionId { get; set; }

        public string PackageTypeDescription { get; set; }

        public int Priority { get; set; }

        public string RegistrationNumber { get; set; }

        [PositiveOrZero]
        public double RequestedQuantity { get; set; }

        public MissionOperationStatus Status { get; set; } = MissionOperationStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionOperationType Type { get; set; }

        #endregion
    }
}
