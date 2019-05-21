﻿using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CellDetails : BaseModel<int>, ICellUpdatePolicy
    {
        #region Fields

        private int? column;

        private int? floor;

        private int? number;

        private int priority;

        private double? xCoordinate;

        private double? yCoordinate;

        private double? zCoordinate;

        #endregion

        #region Properties

        public string AbcClassId { get; set; }

        public int AisleId { get; set; }

        public int AreaId { get; set; }

        public int CellStatusId { get; set; }

        public int? CellTypeId { get; set; }

        public int? Column
        {
            get => this.column;
            set => this.column = CheckIfStrictlyPositive(value);
        }

        public int? Floor
        {
            get => this.floor;
            set => this.floor = CheckIfStrictlyPositive(value);
        }

        public int LoadingUnitsCount { get; set; }

        public int? Number
        {
            get => this.number;
            set => this.number = CheckIfStrictlyPositive(value);
        }

        public int Priority
        {
            get => this.priority;
            set => this.priority = CheckIfStrictlyPositive(value);
        }

        public Side Side { get; set; }

        public double? XCoordinate
        {
            get => this.xCoordinate;
            set => this.xCoordinate = CheckIfPositive(value);
        }

        public double? YCoordinate
        {
            get => this.yCoordinate;
            set => this.yCoordinate = CheckIfPositive(value);
        }

        public double? ZCoordinate
        {
            get => this.zCoordinate;
            set => this.zCoordinate = CheckIfPositive(value);
        }

        #endregion
    }
}
