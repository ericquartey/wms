﻿using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Articolo
    public class Item : ITimestamped, IDataModel
    {
        #region Properties

        public AbcClass AbcClass { get; set; }

        public string AbcClassId { get; set; }

        public int? AverageWeight { get; set; }

        public string Code { get; set; }

        public IEnumerable<Compartment> Compartments { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public int? FifoTimePick { get; set; }

        public int? FifoTimePut { get; set; }

        public double? Height { get; set; }

        public int Id { get; set; }

        public string Image { get; set; }

        public DateTime? InventoryDate { get; set; }

        public int? InventoryTolerance { get; set; }

        public IEnumerable<ItemArea> ItemAreas { get; set; }

        public ItemCategory ItemCategory { get; set; }

        public int? ItemCategoryId { get; set; }

        public IEnumerable<ItemListRow> ItemListRows { get; set; }

        public IEnumerable<ItemCompartmentType> ItemsCompartmentTypes { get; set; }

        public DateTime LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        public double? Depth { get; set; }

        public ItemManagementType ManagementType { get; set; }

        public MeasureUnit MeasureUnit { get; set; }

        public string MeasureUnitId { get; set; }

        public IEnumerable<Mission> Missions { get; set; }

        public string Note { get; set; }

        public int? PickTolerance { get; set; }

        public int? PutTolerance { get; set; }

        public int? ReorderPoint { get; set; }

        public int? ReorderQuantity { get; set; }

        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
