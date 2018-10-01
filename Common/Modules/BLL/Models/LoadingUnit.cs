namespace Ferretto.Common.Modules.BLL.Models
{
    public class LoadingUnit : BusinessObject
    {
        #region Properties

        public string AbcClassDescription { get; set; }
        public string AisleName { get; set; }
        public string AreaName { get; set; }
        public int? CellColumn { get; set; }
        public int? CellFloor { get; set; }
        public int? CellNumber { get; set; }
        public string CellPositionDescription { get; set; }
        public string CellSide { get; set; }
        public string Code { get; set; }
        public int Id { get; set; }
        public string LoadingUnitStatusDescription { get; set; }
        public string LoadingUnitTypeDescription { get; set; }

        #endregion Properties
    }
}
