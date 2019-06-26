using System;
using Ferretto.WMS.Data.Core.Interfaces.Policies;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MissionOperation : BaseModel<int>, IMissionOperationPolicy
    {
        #region Properties

        public int CompartmentId { get; set; }

        public DateTime CreationDate { get; set; }

        [PositiveOrZero]
        public double DispatchedQuantity { get; set; }

        public int ItemId { get; set; }

        public ItemList ItemList { get; set; }

        public int? ItemListId { get; set; }

        public ItemListRow ItemListRow { get; set; }

        public int? ItemListRowId { get; set; }

        public DateTime LastModificationDate { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int MissionId { get; set; }

        public int? PackageTypeId { get; set; }

        public int Priority { get; set; }

        [PositiveOrZero]
        public double QuantityRemainingToDispatch => this.RequestedQuantity - this.DispatchedQuantity;

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
