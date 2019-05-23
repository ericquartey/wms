using System;
using System.Collections.Generic;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRow : BaseModel<int>, IItemListRowDeletePolicy, IItemListRowExecutePolicy, IItemListRowUpdatePolicy
    {
        #region Properties

        [JsonIgnore]
        public int ActiveMissionsCount { get; set; }

        [JsonIgnore]
        public int ActiveSchedulerRequestsCount { get; set; }

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public double DispatchedQuantity { get; set; }

        public string ItemDescription { get; set; }

        public int ItemListId { get; set; }

        public string ItemUnitMeasure { get; set; }

        public IEnumerable<Machine> Machines { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int? Priority { get; set; }

        public double RequestedQuantity { get; set; }

        public ItemListRowStatus Status { get; set; }

        #endregion
    }
}
