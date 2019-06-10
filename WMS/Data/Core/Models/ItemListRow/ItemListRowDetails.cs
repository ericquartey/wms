using System;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRowDetails : BaseModel<int>, IItemListRowDeletePolicy, IItemListRowExecutePolicy
    {
        #region Properties

        [JsonIgnore]
        [PositiveOrZero]
        public int ActiveMissionsCount { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ActiveSchedulerRequestsCount { get; set; }

        public string Code { get; set; }

        public DateTime? CompletionDate { get; set; }

        public DateTime CreationDate { get; set; }

        [PositiveOrZero]
        public double DispatchedQuantity { get; set; }

        public string ItemDescription { get; set; }

        public int ItemId { get; set; }

        public string ItemImage { get; set; }

        public string ItemListCode { get; set; }

        public string ItemListDescription { get; set; }

        public int ItemListId { get; set; }

        public ItemListType ItemListType { get; set; }

        public string ItemUnitMeasure { get; set; }

        public DateTime? LastExecutionDate { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        [Positive]
        public int? Priority { get; set; }

        public string RegistrationNumber { get; set; }

        [Positive]
        public double RequestedQuantity { get; set; }

        public ItemListRowStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion
    }
}
