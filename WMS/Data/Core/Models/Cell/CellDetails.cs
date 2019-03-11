using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CellDetails : BaseModel<int>
    {
        #region Fields

        private int? column;

        private int? floor;

        private int? number;

        private int priority;

        private int? xCoordinate;

        private int? yCoordinate;

        private int? zCoordinate;

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

        public int? XCoordinate
        {
            get => this.xCoordinate;
            set => this.xCoordinate = CheckIfPositive(value);
        }

        public int? YCoordinate
        {
            get => this.yCoordinate;
            set => this.yCoordinate = CheckIfPositive(value);
        }

        public int? ZCoordinate
        {
            get => this.zCoordinate;
            set => this.zCoordinate = CheckIfPositive(value);
        }

        #endregion
    }
}
