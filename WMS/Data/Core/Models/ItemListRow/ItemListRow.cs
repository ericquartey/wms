﻿using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRow : BaseModel<int>
    {
        #region Properties

        public bool CanBeExecuted => this.Status == ItemListRowStatus.Incomplete
                   || this.Status == ItemListRowStatus.Suspended
                   || this.Status == ItemListRowStatus.Waiting;

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public int DispatchedQuantity { get; set; }

        public string ItemDescription { get; set; }

        public int ItemListId { get; set; }

        public string ItemUnitMeasure { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int? Priority { get; set; }

        public int RequestedQuantity { get; set; }

        public ItemListRowStatus Status { get; set; }

        #endregion
    }
}
