﻿using System;
using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Item : BaseModel<int>, IItemWithdrawPolicy, IItemDeletePolicy
    {
        #region Fields

        private int? averageWeight;

        private int? fifoTimePick;

        private int? fifoTimeStore;

        private double? height;

        private int? inventoryTolerance;

        private double? length;

        private int? pickTolerance;

        private int? reorderPoint;

        private int? reorderQuantity;

        private int? storeTolerance;

        private int totalAvailable;

        private int totalReservedForPick;

        private int totalReservedToStore;

        private int totalStock;

        private double? width;

        #endregion

        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        public int? AverageWeight
        {
            get => this.averageWeight;
            set => this.averageWeight = CheckIfStrictlyPositive(value);
        }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public int? FifoTimePick
        {
            get => this.fifoTimePick;
            set => this.fifoTimePick = CheckIfStrictlyPositive(value);
        }

        public int? FifoTimeStore
        {
            get => this.fifoTimeStore;
            set => this.fifoTimeStore = CheckIfStrictlyPositive(value);
        }

        public double? Height
        {
            get => this.height;
            set => this.height = CheckIfStrictlyPositive(value);
        }

        public string Image { get; set; }

        public DateTime? InventoryDate { get; set; }

        public int? InventoryTolerance
        {
            get => this.inventoryTolerance;
            set => this.inventoryTolerance = CheckIfStrictlyPositive(value);
        }

        public string ItemCategoryDescription { get; set; }

        public int? ItemCategoryId { get; set; }

        public int ItemListRowsCount { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastStoreDate { get; set; }

        public double? Length
        {
            get => this.length;
            set => this.length = CheckIfStrictlyPositive(value);
        }

        public IEnumerable<MachineWithdraw> Machines { get; set; }

        public ItemManagementType ManagementType { get; set; }

        public string MeasureUnitDescription { get; set; }

        public string MeasureUnitId { get; set; }

        public int MissionsCount { get; set; }

        public string Note { get; set; }

        public int? PickTolerance
        {
            get => this.pickTolerance;
            set => this.pickTolerance = CheckIfStrictlyPositive(value);
        }

        public int? ReorderPoint
        {
            get => this.reorderPoint;
            set => this.reorderPoint = CheckIfStrictlyPositive(value);
        }

        public int? ReorderQuantity
        {
            get => this.reorderQuantity;
            set => this.reorderQuantity = CheckIfStrictlyPositive(value);
        }

        public int SchedulerRequestsCount { get; set; }

        public int? StoreTolerance
        {
            get => this.storeTolerance;
            set => this.storeTolerance = CheckIfStrictlyPositive(value);
        }

        public int TotalAvailable
        {
            get => this.totalAvailable;
            set => this.totalAvailable = CheckIfPositive(value);
        }

        public int TotalReservedForPick
        {
            get => this.totalReservedForPick;
            set
            {
                this.totalReservedForPick = CheckIfPositive(value);
            }
        }

        public int TotalReservedToStore
        {
            get => this.totalReservedToStore;
            set
            {
                this.totalReservedToStore = CheckIfPositive(value);
            }
        }

        public int TotalStock
        {
            get => this.totalStock;
            set
            {
                this.totalStock = CheckIfPositive(value);
            }
        }

        public double? Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        #endregion
    }
}
