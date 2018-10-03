namespace Ferretto.Common.Modules.BLL.Models
{
    public class LoadingUnit : BusinessObject
    {
        #region Fields

        private int? cellColumn;
        private int? cellFloor;
        private int? cellNumber;

        #endregion Fields

        #region Properties

        public string AbcClassDescription { get; set; }

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        public int? CellColumn
        {
            get => this.cellColumn;
            set => SetIfStrictlyPositive(ref this.cellColumn, value);
        }

        public int? CellFloor
        {
            get => this.cellFloor;
            set => SetIfStrictlyPositive(ref this.cellFloor, value);
        }

        public int? CellNumber
        {
            get => this.cellNumber;
            set => SetIfStrictlyPositive(ref this.cellNumber, value);
        }

        public string CellPositionDescription { get; set; }

        public string CellSide { get; set; }

        public string Code { get; set; }

        public int Id { get; set; }

        public string LoadingUnitStatusDescription { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        #endregion Properties
    }
}
